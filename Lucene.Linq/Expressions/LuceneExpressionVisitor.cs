using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Collections.ObjectModel;

namespace Lucene.Linq.Expressions
{
  internal class LuceneExpressionVisitor : ExpressionVisitor
  {
    protected override Expression Visit(Expression exp)
    {
      if (exp == null)
      {
        return null;
      }

      switch ((LuceneExpressionType)exp.NodeType)
      {
        case LuceneExpressionType.Index:
          return this.VisitIndex((IndexExpression)exp);
        case LuceneExpressionType.Field:
          return this.VisitField((FieldExpression)exp);
        case LuceneExpressionType.Select:
          return this.VisitSelect((SelectExpression)exp);
        case LuceneExpressionType.Projection:
          return this.VisitProjection((ProjectionExpression)exp);
        default:
          return base.Visit(exp);
      }
    }

    protected virtual Expression VisitIndex(IndexExpression index)
    {
      return index;
    }

    protected virtual Expression VisitField(FieldExpression field)
    {
      return field;
    }

    protected virtual Expression VisitSelect(SelectExpression select)
    {
      Expression from = this.VisitSource(select.From);
      Expression where = this.Visit(select.Where);
      ReadOnlyCollection<FieldDeclaration> fields = this.VisitFieldDeclarations(select.Fields);

      if (from != select.From || where != select.Where || fields != select.Fields)
      {
        return new SelectExpression(select.Type, select.Alias, fields, select.DefaultFieldNames, from, where);
      }

      return select;
    }

    protected virtual Expression VisitSource(Expression source)
    {
      return this.Visit(source);
    }

    protected virtual Expression VisitProjection(ProjectionExpression proj)
    {
      SelectExpression source = (SelectExpression)this.Visit(proj.Source);
      Expression projector = this.Visit(proj.Projector);

      if (source != proj.Source || projector != proj.Projector)
      {
        return new ProjectionExpression(source, projector);
      }

      return proj;
    }

    protected ReadOnlyCollection<FieldDeclaration> VisitFieldDeclarations(ReadOnlyCollection<FieldDeclaration> fields)
    {
      List<FieldDeclaration> alternate = null;

      for (int i = 0, n = fields.Count; i < n; i++)
      {
        FieldDeclaration field = fields[i];
        Expression e = this.Visit(field.Expression);

        if (alternate == null && e != field.Expression)
        {
          alternate = fields.Take(i).ToList();
        }

        if (alternate != null)
        {
          alternate.Add(new FieldDeclaration(field.Name, e));
        }
      }

      if (alternate != null)
      {
        return alternate.AsReadOnly();
      }

      return fields;
    }
  }
}