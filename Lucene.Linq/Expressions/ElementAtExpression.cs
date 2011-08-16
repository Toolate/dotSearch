using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Lucene.Linq.Expressions {
    internal class ElementAtExpression : ProjectionExpression {
        public ElementAtExpression(ProjectionExpression proj, Expression indexExpression)
            : base(proj.Source, proj.Projector) {
            this.Index = indexExpression;
        }

        internal Expression Index { get; private set; }
    }
}
