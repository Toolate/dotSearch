using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Data.SqlClient;
using System.Reflection;
using Lucene.Linq.Mapping;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Analysis;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Lucene.Linq.Expressions;
using Lucene.Net.Store;
using System.Threading;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Diagnostics;
using LDirectory = Lucene.Net.Store.Directory;

namespace Lucene.Linq {

    /// <summary>
    /// Index Context contains the Lucene Searcher and Modifier with locking semantics
    /// </summary>
    public class IndexContext: IDisposable {

        private class Writer:IndexWriter, IDisposable {

            public Writer(LDirectory directory, Analyzer analyzer)
                : base(directory, analyzer, false) {
                
            }

            public void Dispose() {
                this.Close();
            }
        }

        #region Fields/Properties

        LDirectory _directory = null;
        Analyzer _defaultAnalyzer = null;
        IndexSearcher _searcher = null;
        IndexModifier _modifier = null;
        ReaderWriterLockSlim _modifyingLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        ///<summary>
        /// The backing Lucene Store which this context is storing
        ///</summary>
        public LDirectory Directory {
            get {
                return _directory;
            }
        }

        /// <summary>
        /// The index searcher
        /// </summary>
        public IndexSearcher Searcher { get {
                using (EnterReadLock()) {
                    return _searcher;
                }
            }
        }

        /// <summary>
        /// The index modifier
        /// </summary>
        public IndexModifier Modifier { get { using (EnterUpgradableReadLock()) { return _modifier;

        } } }

        /// <summary>
        /// The default analyzer
        /// </summary>
        public Analyzer DefaultAnalyzer {
            get { return _defaultAnalyzer; }
        }

        #endregion

        #region Ctors

        /// <summary>
        /// Creates a context from a Lucene backing store
        /// </summary>
        /// <param name="directory">The directory in which to store the index</param>
        public IndexContext(LDirectory directory):this(directory,null) {}

        /// <summary>
        /// Creates a context from a lucene backing store and default analyzer
        /// </summary>
        /// <param name="directory">The directory in which to store the index</param>
        /// <param name="defaultAnalyzer">The default analyzer to use in the context</param>
        public IndexContext(LDirectory directory, Analyzer defaultAnalyzer) {
            if (directory == null) {
                throw new ArgumentNullException("directory");
            }
            _directory = directory;

            // Null analyzer is okay
            _defaultAnalyzer = defaultAnalyzer;

            // build searcher refs
            ConstructModifier();
            ConstructSearchers();
        }

        #endregion

        #region Modifying Lock Semantics

        /// <summary>
        /// Acquire a write lock (if not already taken) that doesn't refresh searches on disposal
        /// </summary>
        /// <returns>Disposable lock</returns>
        public IDisposable EnterWriteLockWithoutSearcherRefresh() {
            if (_modifyingLock.IsWriteLockHeld) {
                return new ActionDisposable(() => { });

            } else {
                var wl = _modifyingLock.WriteLock();
                return new ActionDisposable(() => {
                    wl.Dispose();
                });
            }
        }

        /// <summary>
        /// Acquires a read lock (if not already taken)
        /// </summary>
        /// <returns>Disposable lock</returns>
        public IDisposable EnterReadLock() {
            if (_modifyingLock.IsReadLockHeld) {
                return new ActionDisposable(() => { });
            } else {
                var wl = _modifyingLock.ReadLock();
                return new ActionDisposable(() => {
                    wl.Dispose();
                });
            }
        }

        /// <summary>
        /// Acquires a read lock (if not already taken) that can be upgraded to a write lock
        /// </summary>
        /// <returns>Disposable lock</returns>
        public IDisposable EnterUpgradableReadLock() {
            if (_modifyingLock.IsUpgradeableReadLockHeld) {
                return new ActionDisposable(() => { });
            } else {
                var wl = _modifyingLock.UpgradableReadLock();
                return new ActionDisposable(() => wl.Dispose());
            }
        }

        /// <summary>
        /// Acquires a write lock (if not already taken)
        /// </summary>
        /// <returns>Disposable lock</returns>
        public IDisposable EnterWriteLock() {
            if (_modifyingLock.IsWriteLockHeld) {
                return new ActionDisposable(() => { });
            } else {
                var wl = _modifyingLock.WriteLock();
                return new ActionDisposable(() => {
                    RefreshSearchers();
                    wl.Dispose();
                });
            }
        }

        #endregion

        #region Index Merging

        /// <summary>
        /// Merges the Lucene directories into the base index
        /// </summary>
        /// <param name="directories">Directories to merge into this index</param>
        public void Merge(LDirectory[] directories) {

            if (directories == null)
                return;
            if (directories.Length == 0)
                return;

            using (EnterWriteLock()) {
                using (var w = new Writer(this._directory, this._defaultAnalyzer ?? new StandardAnalyzer())) {

                    w.AddIndexes(directories);

                }
            }

        }

        #endregion

        #region IndexSearchers Management

        private void ConstructSearchers() {
            using (EnterWriteLockWithoutSearcherRefresh()) {
                _searcher = new IndexSearcher(_directory);
            }
        }

        private void RefreshSearchers() {

            // acquiring a writer lock will start queuing all search (reader) requests
            using (_modifyingLock.WriteLock()) {
                CloseSearchers();
                ConstructSearchers();
            }

        }

        private void CloseSearchers() {
            using (EnterWriteLockWithoutSearcherRefresh()) {
                if (_searcher != null) {
                    _searcher.Close();
                    _searcher = null;
                }
            }
        }

        #endregion

        #region IndexModifier Management

        private void ConstructModifier() {

            if (_directory == null) throw new InvalidOperationException("Directory is not specified");
            
            using (EnterWriteLockWithoutSearcherRefresh()) {
                try {
                    _modifier = new IndexModifier(_directory, _defaultAnalyzer ?? new SimpleAnalyzer(), !DoesIndexExist());

                } catch (Exception ex) {
                    var _writer = new IndexWriter(_directory, _defaultAnalyzer ?? new SimpleAnalyzer(), true);
                    _writer.Close();
                    _modifier = new IndexModifier(_directory, _defaultAnalyzer ?? new SimpleAnalyzer(), false);
                }
            }
            
        }

        /// <summary>
        /// REVIEW: make private?
        /// </summary>
        public void RefreshModifier() {

            // acquiring a writer lock will start queuing all search (reader) requests
            using (EnterWriteLock()) {
                CloseModifier();
                ConstructModifier();
            }

        
        }

        private void CloseModifier() {
            
                using (EnterWriteLockWithoutSearcherRefresh()) {
                    if (_modifier != null) {
                        _modifier.Close();
                        _modifier = null;
                    }
                }
         
        }

        #endregion

        #region Utility
        private bool DoesIndexExist() {
            return IndexReader.IndexExists(_directory);
        }

        #endregion

        #region IDisposable Members

        ///<summary>
        /// Closes the context - searchers, modifiers and leaves the directory in tact
        ///</summary>
        public void Dispose() {

            using (EnterWriteLockWithoutSearcherRefresh()) {
                CloseSearchers();
                CloseModifier();
            }
        }

        #endregion
    }
}
