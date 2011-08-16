using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using Lucene.Linq.Expressions;using System.Threading;
using Lucene.Linq.Mapping;
namespace Lucene.Linq {

    ///<summary>A set of indexes</summary>
    public class IndexSet : IIndexSet {

        #region Fields

        private Dictionary<Type,IIndex> _indexes;
        private ReaderWriterLockSlim _indexesLock;
        
        private DirectoryInfo _directory;

        #endregion

        #region Properties

        public int Count {
            get {
                using (_indexesLock.ReadLock()) {
                    return _indexes.Count;
                }
            }
        }

        #endregion

        #region Ctors/Init

        ///<summary>Create set in file system directory</summary>
        ///<param name="directory">The index set directory info</param>
        public IndexSet(DirectoryInfo directory)
        {
            if (directory == null) {
                throw new ArgumentNullException("directory");
            }
            this._directory = directory;
            Initialize();
        }

        ///<summary>Create set in file system directory</summary>
        ///<param name="path">The index set file system path</param>
        public IndexSet(string path)
        {
            if (String.IsNullOrEmpty(path)) {
                throw new ArgumentNullException("path");
            }
            this._directory = new DirectoryInfo(path);
            Initialize();
        }

        /// <summary>Create set in a RAM directory</summary>
        public IndexSet() {
            this._directory = null;
            Initialize();
        }

        private void Initialize() {

            if (_directory != null) {
                if (!_directory.Exists) {
                    _directory.Create();
                }
            }

            _indexes = new Dictionary<Type, IIndex>();
            _indexesLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        }

        #endregion

        #region Add

        /// <summary>Adds an index to the set</summary>
        /// <typeparam name="TIndex">Type of the index to add</typeparam>
        public void Add<TIndex>() {
            var t = typeof(TIndex);
            Add(t);
        }

        /// <summary>Adds an index to the set</summary>
        /// <param name="indexType">Type of the index to add</param>
        public void Add(Type indexType) {
            
            var da = indexType.GetDocumentDetails();
            string dirName = da == null ? indexType.Name : da.Name ?? indexType.Name;

            using (_indexesLock.UpgradableReadLock()) {
                // get the index directory name
                if (_indexes.ContainsKey(indexType)) {
                    throw new ArgumentNullException("indexType","index of that type already exists");

                } else {
                    using (_indexesLock.WriteLock()) {

                        IIndex context = null;

                        // Make the generic Index<T> type
                        var genericIndexType = typeof(Index<>).MakeGenericType(indexType);

                        // if RAM indexSet, create the instance
                        if (_directory == null) {

                            // create the Index instance using the parameterless ctor
                            context = Activator.CreateInstance(genericIndexType) as IIndex;

                        } else {
                            // get full path
                            string path = Path.Combine(_directory.FullName, dirName);

                            // create the index using the single string parameter ctor
                            context =  Activator.CreateInstance(genericIndexType, path) as IIndex;
                        }

                        // add the index to the set
                        _indexes.Add(indexType, context);
                    }
                }
            }
        }

        #endregion

        #region Remove

        /// <summary>Removes an index from the set</summary>
        /// <typeparam name="TIndex">the type of the index</typeparam>
        public void Remove<TIndex>() {
            Remove(typeof (TIndex));
        }

        /// <summary>Removes an index from the set</summary>
        /// <param name="indexType">the type of the index</param>
        public void Remove(Type indexType) {

            var idx = Get(indexType);

            if (idx==null)
                throw new ArgumentException("That index type is not in the set","indexType");

            using (_indexesLock.WriteLock()) {

                // Remove from the indexes table
                if (_indexes.Remove(indexType)) {

                    // Dispose of the index
                    if (idx is IDisposable) {
                        (idx as IDisposable).Dispose();
                    }
                } else {
                    throw new ApplicationException(String.Format("Could not remove the index type {0} for an unknown reason. Probably some lower level locking getting in the way",indexType.Name));
                }
            }

        }

        #endregion

        #region Get/Indexer

        /// <summary>
        /// Gets the IIndex based on the type
        /// </summary>
        /// <param name="t">the key type</param>
        /// <returns>found index reference, otherwise null</returns>
        public IIndex this[Type t] {
            get {
                return Get(t);
            }
        }


        /// <summary>
        /// Gets the index of a particular type from the set. Exception is thrown if the TIndex isn't stored in the type
        /// </summary>
        /// <typeparam name="TIndex">Type of the index</typeparam>
        /// <returns>Instance of the index</returns>
        public IIndex<TIndex> Get<TIndex>() {
            var t = typeof(TIndex);
            return Get(t) as IIndex<TIndex>;
        }

        /// <summary>
        /// Gets the index of a particular type from the set. Exception is thrown if the TIndex isn't stored in the type
        /// </summary>
        /// <param name="t">Type of the index</param>
        /// <returns>Instance of the index</returns>
        public IIndex Get(Type t) {
            using (_indexesLock.ReadLock()) {
                if (_indexes.ContainsKey(t)) {
                    var context = _indexes[t];
                    return context;
                } else {
                    throw new ArgumentException("TIndex isn't being indexed", "t");
                }
            }
        }

        /// <summary>Returns all the indexes in the set</summary>
        public IEnumerable<IIndex> All() {
            using (_indexesLock.ReadLock()) {
                return _indexes.Values.ToList().AsEnumerable();
            }
        }
        #endregion

    }
}
