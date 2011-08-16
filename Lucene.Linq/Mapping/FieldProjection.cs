using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Lucene.Linq.Mapping
{
  internal class FieldProjection
  {
    internal string Fields;
    internal Expression Selector;
  }
}
