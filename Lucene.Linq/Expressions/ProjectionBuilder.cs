using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using Lucene.Linq.Mapping;

namespace Lucene.Linq.Expressions {

    internal class ProjectionBuilder : LuceneExpressionVisitor, IProjectionBuilder {
        protected ParameterExpression documentParameter;
        protected string documentAlias;
        protected static MethodInfo miGetValue;
        protected static MethodInfo miExecuteSubQuery;

        internal ProjectionBuilder() {
            if (miGetValue == null) {
                miGetValue = typeof(DocumentProjection).GetMethod("GetValue");
                miExecuteSubQuery = typeof(DocumentProjection).GetMethod("ExecuteSubQuery");
            }
        }

        public virtual LambdaExpression Build(Expression expression, string alias) {
            this.documentParameter = Expression.Parameter(typeof(DocumentProjection), "document");
            this.documentAlias = alias;

            // visit the body
            Expression body = this.Visit(expression);

            // return a lambda wrapping the 
            return Expression.Lambda(body, this.documentParameter);
        }

        protected override Expression VisitField(FieldExpression field) {
            if (field.Alias == this.documentAlias) {
                return Expression.Convert(Expression.Call(this.documentParameter, miGetValue, Expression.Constant(field.Name), Expression.Constant(field.Type)), field.Type);
            }

            return field;
        }

        protected override Expression VisitProjection(ProjectionExpression projection) {
            LambdaExpression subQuery = Expression.Lambda(base.VisitProjection(projection), this.documentParameter);
            Type elementType = TypeSystem.GetElementType(subQuery.Body.Type);
            MethodInfo mi = miExecuteSubQuery.MakeGenericMethod(elementType);
            return Expression.Convert(Expression.Call(this.documentParameter, mi, Expression.Constant(subQuery)), projection.Type);
        }
    }
}