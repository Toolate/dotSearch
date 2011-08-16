using System;
using Lucene.Net;
using System.Collections;
using System.Collections.Generic;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Linq.Expressions;
using System.Linq.Expressions;
using System.Linq;
using System.IO;

namespace Lucene.Linq {

    /// <summary>
    /// Generic index interface
    /// </summary>
    /// <typeparam name="TIndex">The type of the object to store in the index</typeparam>
    public interface IIndex<TIndex> : IIndex, IQueryable<TIndex>, IQueryProvider, IDisposable {

        /// <summary>
        /// Add an object into the index
        /// </summary>
        /// <param name="item">The item to add</param>
        new void Add(TIndex item);

        /// <summary>
        /// Add a sequence of objects into the index
        /// </summary>
        /// <param name="items">The items to add</param>
        new void Add(IEnumerable<TIndex> items);

        /// <summary>
        /// Delete an object from the index by key
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <param name="value">The value of the key of which to delete</param>
        new void Delete<TKey>(TKey value);

        /// <summary>
        /// Delete a number of objects from the index by keys
        /// </summary>
        /// <typeparam name="TKey">The type of the keys</typeparam>
        /// <param name="value">The keys of the documents which are to be deleted</param>
        new void Delete<TKey>(IEnumerable<TKey> value);
    }
}
