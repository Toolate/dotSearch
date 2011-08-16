using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Lucene.Linq.Expressions {
    
    internal class SkipExpression : ProjectionExpression {
        public SkipExpression(ProjectionExpression proj, Expression skipAmount)
            : base(proj.Source, proj.Projector) {
            this.SkipAmount = skipAmount;
        }

        internal Expression SkipAmount { get; private set; }

    }
    internal class TakeExpression : SkipExpression {

        internal Expression TakeAmount { get; private set; }

        internal TakeExpression(ProjectionExpression proj, Expression skipAmount, Expression takeAmount)
            : base(proj,skipAmount) {
            this.TakeAmount = takeAmount;
        }



    }
}
