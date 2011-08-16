using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using Lucene.Linq.Expressions;
using System.Threading;
using Lucene.Linq.Mapping;

namespace Lucene.Linq {

    /// <summary>
    /// A set of indexes
    /// </summary>
    public interface IIndexSet {

        /// <summary>Add new Index</summary>
        /// <param name="indexType">type of document the index stores</param>
        void Add(Type indexType);
        /// <summary>Add new Index</summary>
        /// <typeparam name="TIndex">type of document the index stores</typeparam>
        void Add<TIndex>();

        /// <summary>Returns all the Indexes in the set</summary>
        IEnumerable<IIndex> All();

        /// <summary>The number of indexes in the set</summary>
        int Count { get; }

        /// <summary>
        /// Gets the IIndex based on the type
        /// </summary>
        /// <param name="t">the key type</param>
        /// <returns>found index reference, otherwise null</returns>
        IIndex this[Type t] { get; }

        /// <summary>
        /// Gets the index of a particular type from the set. Exception is thrown if the TIndex isn't stored in the type
        /// </summary>
        /// <param name="t">Type of the index</param>
        /// <returns>Instance of the index</returns>
        IIndex Get(Type t);

        /// <summary>
        /// Gets the index of a particular type from the set. Exception is thrown if the TIndex isn't stored in the type
        /// </summary>
        /// <typeparam name="TIndex">Type of the index</typeparam>
        /// <returns>Instance of the index</returns>
        IIndex<TIndex> Get<TIndex>();

        /// <summary>Removes an index from the set</summary>
        /// <param name="indexType">the type of the index</param>
        void Remove(Type indexType);

        /// <summary>Removes an index from the set</summary>
        /// <typeparam name="TIndex">the type of the index</typeparam>
        void Remove<TIndex>();
        
    }
}
