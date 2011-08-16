//====================================================================
// Author: Jason Plante
// Date Created: 24 October 2007
// LINQ to Lucene: Copyright (c) 2007.  All rights reserved.
//====================================================================

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

namespace Lucene.Linq
{
    
    /// <summary>
    /// Manages access to and from an homogenous Index.  
    /// Performs index querying and abstracts object transformation
    /// Constructs index and provides a CRUD interface into an index
    /// Manages access to index properties and document properties
    /// 
    /// TODO: supply a bulk object indexing strategy, based on how much ram to use to store it, CPU usage, etc
    /// </summary>
    public class Index<TEntity> : IIndex<TEntity> {
                
        #region Fields

        //state
        private bool _isDisposed;

        // lucene directory
        private LDirectory _directory;

        // context
        private IndexContext _context;
        
        // reflected properties and field details
        private DocumentDetails _documentDetails;
        private IEnumerable<FieldDetails> _fieldDetails;

        #endregion

        #region Properties

        /// <summary>The Index Storage Context</summary>
        public IndexContext Context
        {
            get { return _context; }
        }

        /// <summary>The name of the Index</summary>
        public string Name {
            get {
                return string.IsNullOrEmpty(_documentDetails.Name) ? typeof(TEntity).Name : _documentDetails.Name;
            }
        }

        /// <summary>The Directory storage instance</summary>
        public LDirectory Directory {
            get {
                return _directory;
            }
        }

        /// <summary>The number of documents in in the index</summary>
        public int Count {
            get{
                    return Context.Modifier.DocCount();
            }
        }

        #endregion

        #region Constructors

        /// <summary>Creates a reference to an Index</summary>
        /// <param name="directory">The storage location of the root of the Index</param>
        public Index(DirectoryInfo directory)
        {
            if (directory == null) {
                throw new ArgumentNullException("directory");
            }


            if (!directory.Exists) {
                directory.Create();
            }
            _directory = FSDirectory.GetDirectory(directory.FullName, false);

            Initialize();
        }

        /// <summary>Creates a reference to an Index stored in FileSystem</summary>
        /// <param name="path">The path of the storage location of the root of the Index</param>
        public Index(string path):this(new DirectoryInfo(path))
        {
        }

        /// <summary>
        /// Creates a reference to an RAM backed Index
        /// </summary>
        public Index() {
            _directory = new RAMDirectory();
            Initialize();
        }

        private void Initialize()
        {
            ReflectEntityAttributes();
            _context = new IndexContext(_directory);
        }
        #endregion
        

        #region Other Methods

        private void ReflectEntityAttributes() {

            var t = typeof(TEntity);
            _documentDetails = t.GetDocumentDetails();
            _fieldDetails = t.GetFieldDetails();
        }

       
        private Document ConvertToDocument(TEntity item) {
            Document document = new Document();
            Type et = typeof(TEntity);
            

            foreach (var p in _fieldDetails) {
                string fieldName = p.Field.Name ?? p.Property.Name;

                // get the value of the property
                object fieldValue = p.Property.GetValue(item,null);

                // type convert it to a string
                string fieldStringValue = ConvertPropertyValueToString(p.Property,fieldValue);

                // add field to doc
                //TODO: add term vector field storage param
                var f = new Field(fieldName, fieldStringValue, p.Field.Store, p.Field.Index);
                f.SetBoost(p.Field.Boost);
                document.Add(f);
            }

            return document;
        }

        private string ConvertPropertyValueToStringWithTokenizer(PropertyInfo property, object value, string fieldName, Analyzer analyzer) {

            if (value == null)
                return "";

            Func<string, string> extractFirstToken = (input) => {

                string[] tokens = input.Tokenize(fieldName, analyzer);
                if (tokens.Length>1) {
                    Console.WriteLine("ERROR? Not sure what to do with multiple tokens, just using the first for now");
                }

                                                           return tokens[0];
                                                       };

            if (property.PropertyType == typeof(string)) {

                return extractFirstToken((string) value);
            }

            // component model conversions

            TypeConverter converter = TypeDescriptor.GetConverter(property.PropertyType);
            if (converter.CanConvertTo(typeof(string))) {
                return extractFirstToken((string) converter.ConvertTo(value, typeof (string)));
            } else {
                try {
                    return extractFirstToken(value.ToString());
                } catch (Exception) {
                    return "";
                }
            }
        }

        private string ConvertPropertyValueToString(PropertyInfo property, object value) {

            if (value == null)
                return "";
            
            if (property.PropertyType == typeof(string))
                return (string)value;

            // component model conversions

            TypeConverter converter = TypeDescriptor.GetConverter(property.PropertyType);
            if (converter.CanConvertTo(typeof(string))) {
                return (string)converter.ConvertTo(value, typeof(string));
            } else {
                try {
                    return value.ToString();
                } catch (Exception) {
                    return "";
                }
            }
        }

        #endregion
        
        #region IIndex<TEntity> Members

        #region Add

        private void Add(TEntity item, bool flush) {
            using (_context.EnterWriteLock()) {
                Analyzer analyzer = _documentDetails.Analyzer;

                // convert TEntity to document
                var doc = ConvertToDocument(item);
                Debug.Assert(doc != null, "Document shouldnt be null");

                // add to index
                _context.Modifier.AddDocument(doc, analyzer);

                // flush if asked
                if (flush)
                    _context.Modifier.Flush();
            }
 
        }

        public virtual void Add(TEntity item) {
            Add(item, true);
        }

        public virtual void Add(IEnumerable<TEntity> items) {

            using (_context.EnterWriteLock()) {
                foreach (var item in items) {
                    Add(item,false);
                }
                _context.Modifier.Flush();
            }
        }

        #endregion

        #region Delete


        private Term ConvertKeyValue<TKey>(TKey key) {
            
            // from the key, get the field
            var keyType = typeof (TKey);

            var matchingFields = from f in _fieldDetails
                                 where f.Property.PropertyType == keyType
                                       && f.Field.IsKey == true
                                 select f;
            if (matchingFields.Count()==0) {
                throw new ArgumentException(@"The key type is not valid. 
                                              The type does not match the property with the FieldAttribute and IsKey==true
                                            ");

            }

            var mfield = matchingFields.First(); // first matching field 
            var analyzer = AnalyzerTypeCache.Instance[mfield.Field.Analyzer ?? Defaults.FieldAnalyzerType];


            return new Term(mfield.Name,ConvertPropertyValueToStringWithTokenizer(mfield.Property,key,mfield.Name,analyzer));
        }
        
        
        private void DeleteDocs<TKey>(TKey key, bool flush) {
            using (_context.EnterWriteLock()) {

                
                // convert the key to a Term
                Term term = ConvertKeyValue(key);

                _context.Modifier.DeleteDocuments(term);
                if (flush) {
                    _context.Modifier.Flush();
                }
            }
        }

        private void Delete(int docNum,bool flush) {
            using (_context.EnterWriteLock()) {
                _context.Modifier.DeleteDocument(docNum);
                if (flush) {
                    _context.Modifier.Flush();
                }
            }
        }

        public virtual void Delete(int docNum) {
            Delete(docNum,true);
        }

        public virtual void Delete(IEnumerable<int> docNums) {
            using (_context.EnterWriteLock()) {
                foreach (var dc in docNums) {
                    Delete(dc, false);
                }
                _context.Modifier.Flush();
            }
        }

        public virtual void Delete<TKey>(TKey value) {
            DeleteDocs(value, true);
        }

        public virtual void Delete<TKey>(IEnumerable<TKey> values) {
            using (_context.EnterWriteLock()) {
                foreach (TKey dc in values) {
                    DeleteDocs(dc, false);
                }
                _context.Modifier.Flush();
            }
        }

        #endregion

        #endregion

        #region IIndex Members

        public void Add(object item) {
            Add((TEntity)item);

        }

        public void Add(System.Collections.IEnumerable items) {
            Add((IEnumerable<TEntity>) items);
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Implements the IDisposable Dispose method that cleans up system resources
        /// </summary>
        /// <param name="disposing">Whether to dispose the resources</param>
        private void Dispose(bool disposing) {
            // Check to see if Dispose has already been called.
            if (!_isDisposed) {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing) {

                    //
                    // Dispose managed resources.
                    //

                    if (_context != null) {
                        _context.Dispose();
                        _context = null;
                    }

                    if (_directory != null) {
                        _directory.Close();
                        _directory = null;
                    }
                        
                        
                }

                // Note disposing has been done.
                _isDisposed = true;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Index() {
            Dispose(false);
        }

        #endregion

        #region IQueryable Members

        public Type ElementType {
            get {
                return typeof(TEntity);
            }
        }

        public System.Linq.Expressions.Expression Expression {
            get {
                return System.Linq.Expressions.Expression.Constant(this);
            }
        }

        public IQueryProvider Provider {
            get {
                return this;
            }
        }

        #endregion


        #region IEnumerable Members

        public System.Collections.IEnumerator GetEnumerator() {
            return GetEnumerator();
        }

        #endregion

        #region IEnumerable<TEntity> Members

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() {
            return ((IEnumerable<TEntity>)
                        (new QueryProvider(Context))
                            .Execute(Expression.Constant(this)))
                        .GetEnumerator();
        }

        #endregion

        #region IQueryProvider Members

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) {
            if (expression == null) {
                throw new ArgumentNullException("expression");
            }
            if (!typeof(IQueryable<TEntity>).IsAssignableFrom(expression.Type)) {
                throw new ArgumentException("expected type: expression should be IQueryable<TElement>");
            }
            return new Query<TElement>(_context, expression);
        }

        public IQueryable CreateQuery(Expression expression) {
            if (expression == null) {
                throw new ArgumentNullException("expression");
            }
            Type elementType = TypeSystem.GetElementType(expression.Type);
            Type type2 = typeof(IQueryable<>).MakeGenericType(new Type[] { elementType });
            if (!type2.IsAssignableFrom(expression.Type)) {
                throw new ArgumentException("expected type: expression should be " + type2.Name);
            }
            return (IQueryable)Activator.CreateInstance(typeof(Query<>).MakeGenericType(new Type[] { elementType }), new object[] { Provider, expression });
        }

        public TResult Execute<TResult>(Expression expression) {
            return (TResult)(new QueryProvider(Context)).Execute(expression);
        }

        public object Execute(Expression expression) {
            return new QueryProvider(Context).Execute(expression);
        }

        #endregion
    }
}
