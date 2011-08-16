using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Lucene.Linq.Expressions
{
  internal class SelectExpression : Expression
  {
    string alias;
    ReadOnlyCollection<FieldDeclaration> fields;
    string[] defaultFieldNames;
    Expression from;
    Expression where;

    internal SelectExpression(Type type, string alias, IEnumerable<FieldDeclaration> fields, string[] defaultFieldNames, Expression from, Expression where)
      : base((ExpressionType)LuceneExpressionType.Select, type)
    {
      this.alias = alias;
      this.fields = fields as ReadOnlyCollection<FieldDeclaration>;
      if (this.fields == null)
        this.fields = new List<FieldDeclaration>(fields).AsReadOnly();

      this.defaultFieldNames = defaultFieldNames;

      this.from = from;
      this.where = where;
    }

    internal string Alias
    {
      get { return this.alias; }
    }

    internal ReadOnlyCollection<FieldDeclaration> Fields
    {
      get { return this.fields; }
    }

    internal string[] DefaultFieldNames
    {
      get { return this.defaultFieldNames; }
    }

    internal Expression From
    {
      get { return this.from; }
    }

    internal Expression Where
    {
      get { return this.where; }
    }
  }
}
