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
using Lucene.Net.Store;

namespace Lucene.Linq {

    ///<summary>An non-generic index interface</summary>
    public interface IIndex : IQueryable{

        /// <summary>Name of the index</summary>
        string Name { get; }

        /// <summary>Number of documents in the index</summary>
        int Count { get; }

        /// <summary>Index Context</summary>
        IndexContext Context { get; }

        /// <summary>Index Store</summary>
        Directory Directory { get; }
        
        /// <summary>Add an object into the index</summary>
        /// <param name="item">item to index</param>
        void Add(object item);

        /// <summary>Add a sequence of objects into the index</summary>
        /// <param name="items">sequence of items to index</param>
        void Add(IEnumerable items);

        /// <summary>Delete item from index by document number</summary>
        /// <param name="documentNumber">Document number to delete from index</param>
        void Delete(int documentNumber);

        /// <summary>Delete items from index by document numbers</summary>
        /// <param name="documentNumbers">Document number sequence to delete from index</param>
        void Delete(IEnumerable<int> documentNumbers);
    }

    

}
