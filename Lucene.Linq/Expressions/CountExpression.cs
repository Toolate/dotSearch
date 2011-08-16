using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Lucene.Linq.Expressions {
    internal class CountExpression : ProjectionExpression {
        public CountExpression(ProjectionExpression proj) : base(proj.Source, proj.Projector) { 
            
        }
    }

    
    
    
}
