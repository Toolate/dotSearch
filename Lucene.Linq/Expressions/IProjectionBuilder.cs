using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Lucene.Linq.Expressions {
    public interface IProjectionBuilder {
        LambdaExpression Build(Expression expression, string alias);
    }
}
