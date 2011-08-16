using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Linq.Expressions;
using System.Linq.Expressions;
using System.Reflection;
using Lucene.Net.Analysis.Standard;

namespace Lucene.Linq.Mapping
{
  internal class FieldProjector : LuceneExpressionVisitor
  {
    Nominator nominator;
    Dictionary<FieldExpression, FieldExpression> map;
    List<FieldDeclaration> fields;
    HashSet<string> fieldNames;
    HashSet<Expression> candidates;
    string existingAlias;
    string newAlias;
    int iField;

    internal FieldProjector(Func<Expression, bool> fnCanBeField)
    {
      this.nominator = new Nominator(fnCanBeField);
    }

    internal ProjectedFields ProjectFields(Expression expression, string newAlias, string existingAlias)
    {
      this.map = new Dictionary<FieldExpression, FieldExpression>();
      this.fields = new List<FieldDeclaration>();
      this.fieldNames = new HashSet<string>();
      this.newAlias = newAlias;
      this.existingAlias = existingAlias;
      this.candidates = this.nominator.Nominate(expression);
      return new ProjectedFields(this.Visit(expression), this.fields.AsReadOnly());
    }

    protected override Expression Visit(Expression expression)
    {
      if (this.candidates.Contains(expression))
      {
        if (expression.NodeType == (ExpressionType)LuceneExpressionType.Field)
        {
          FieldExpression field = (FieldExpression)expression;
          FieldExpression mapped;

          if (this.map.TryGetValue(field, out mapped))
          {
            return mapped;
          }

          if (this.existingAlias == field.Alias)
          {
            int ordinal = this.fields.Count;
            string fieldName = this.GetUniqueFieldName(field.Name);
            this.fields.Add(new FieldDeclaration(fieldName, field));
            mapped = new FieldExpression(field.Type, this.newAlias, fieldName, ordinal, field.Analyzer);
            this.map[field] = mapped;
            this.fieldNames.Add(fieldName);
            return mapped;
          }

          // must be referring to outer scope
          return field;
        }

        else
        {
          string fieldName = this.GetNextFieldName();
          int ordinal = this.fields.Count;
          this.fields.Add(new FieldDeclaration(fieldName, expression));
          return new FieldExpression(expression.Type, this.newAlias, fieldName, ordinal, new StandardAnalyzer());
        }
      }
      else
      {
        return base.Visit(expression);
      }
    }

    private bool IsFieldNameInUse(string name)
    {
      return this.fieldNames.Contains(name);
    }

    private string GetUniqueFieldName(string name)
    {
      string baseName = name;
      int suffix = 1;

      while (this.IsFieldNameInUse(name))
      {
        name = baseName + (suffix++);
      }

      return name;
    }

    private string GetNextFieldName()
    {
      return this.GetUniqueFieldName("c" + (iField++));
    }

    class Nominator : LuceneExpressionVisitor
    {
      Func<Expression, bool> fnCanBeField;
      bool isBlocked;
      HashSet<Expression> candidates;

      internal Nominator(Func<Expression, bool> fnCanBeField)
      {
        this.fnCanBeField = fnCanBeField;
      }

      internal HashSet<Expression> Nominate(Expression expression)
      {
        this.candidates = new HashSet<Expression>();
        this.isBlocked = false;
        this.Visit(expression);
        return this.candidates;
      }

      protected override Expression Visit(Expression expression)
      {
        if (expression != null)
        {
          bool saveIsBlocked = this.isBlocked;
          this.isBlocked = false;

          base.Visit(expression);
          if (!this.isBlocked)
          {
            if (this.fnCanBeField(expression))
            {
              this.candidates.Add(expression);
            }
            else
            {
              this.isBlocked = true;
            }
          }
          this.isBlocked |= saveIsBlocked;
        }
        return expression;
      }
    }
  }
}
