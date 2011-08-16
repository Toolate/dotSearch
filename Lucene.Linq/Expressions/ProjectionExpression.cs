using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Lucene.Linq.Expressions {
    internal class ProjectionExpression : Expression {
        readonly SelectExpression _source;
        readonly Expression _projector;
        
        internal ProjectionExpression(SelectExpression source, Expression projector)
            : base((ExpressionType)LuceneExpressionType.Projection, source.Type) {
            this._source = source;
            this._projector = projector;
        }

        internal SelectExpression Source {
            get { return this._source; }
        }

        internal Expression Projector {
            get { return this._projector; }
        }


    }
}