using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Lucene.Net.Search;

namespace Lucene.Linq.Expressions {

    ///<summary>
    /// LINQ to Lucene Query Translation
    ///</summary>
    public interface IQueryTranslator {
        ///<summary>Translate the Linq Expression into a Lucene Querysummary</summary>
        ///<param name="expression">The expression to translate</param>
        ///<returns>The Lucene.Net Query</returns>
        Query Translate(Expression expression);
    }
}
