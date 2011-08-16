using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucene.Linq.Expressions
{
  internal enum LuceneExpressionType
  {
    Index = 1000,  // make sure these don't overlap with ExpressionType
    Field,
    Select,
    Projection
  }
}
