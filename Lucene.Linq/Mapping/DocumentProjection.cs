using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Lucene.Linq.Mapping {

    public abstract class DocumentProjection {



        public abstract object GetValue(string fieldName, Type fieldType);
        public abstract IEnumerable<E> ExecuteSubQuery<E>(LambdaExpression query);
    }
}
