using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Lucene.Linq.Expressions
{
  internal class FieldDeclaration
  {
    string name;
    Expression expression;
    internal FieldDeclaration(string name, Expression expression)
    {
      this.name = name;
      this.expression = expression;
    }

    internal string Name
    {
      get { return this.name; }
    }

    internal Expression Expression
    {
      get { return this.expression; }
    }
  }
}
