using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Lucene.Linq.Expressions
{
  internal class IndexExpression : Expression
  {
    string alias;
    string name;

    internal IndexExpression(Type type, string alias, string name)
      : base((ExpressionType)LuceneExpressionType.Index, type)
    {
      this.alias = alias;
      this.name = name;
    }



    internal string Alias
    {
      get { return this.alias; }
    }

    internal string Name
    {
      get { return this.name; }
    }
  }

  internal static class IndexExpressionExtensions
  {
    internal static bool IsIndexExpression(this ExpressionType et)
    {
      return ((int)et) >= 1000;
    }
  }
}
