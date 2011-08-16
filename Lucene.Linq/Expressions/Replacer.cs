using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Lucene.Linq.Expressions
{
  internal class Replacer : LuceneExpressionVisitor
  {
    Expression searchFor;
    Expression replaceWith;

    internal Expression Replace(Expression expression, Expression searchFor, Expression replaceWith)
    {
      this.searchFor = searchFor;
      this.replaceWith = replaceWith;

      return this.Visit(expression);
    }

    protected override Expression Visit(Expression exp)
    {
      if (exp == this.searchFor)
      {
        return this.replaceWith;
      }
    
      return base.Visit(exp);
    }
  }

}
