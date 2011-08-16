using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Lucene.Linq.Mapping;
using System.Reflection;
using Lucene.Net.Analysis;

namespace Lucene.Linq.Expressions {
    internal class QueryBinder : ExpressionVisitor {

        FieldProjector fieldProjector;
        Dictionary<ParameterExpression, Expression> map;
        private AnalyzerTypeCache typeCache;
        
        int aliasCount;

        #region Ctors

        internal QueryBinder() {
            this.fieldProjector = new FieldProjector(this.CanBeField);
            typeCache = AnalyzerTypeCache.Instance;
        }

        #endregion

        private bool CanBeField(Expression expression) {
            return expression.NodeType == (ExpressionType)LuceneExpressionType.Field;
        }

        internal Expression Bind(Expression expression) {
            this.map = new Dictionary<ParameterExpression, Expression>();
            return this.Visit(expression);
        }

        private static Expression StripQuotes(Expression e) {
            while (e.NodeType == ExpressionType.Quote) {
                e = ((UnaryExpression)e).Operand;
            }

            return e;
        }

        private string GetNextAlias() {
            return "t" + (aliasCount++);
        }

        private ProjectedFields ProjectFields(Expression expression, string newAlias, string existingAlias) {
            return this.fieldProjector.ProjectFields(expression, newAlias, existingAlias);
        }

        protected override Expression VisitMethodCall(MethodCallExpression m) {
            if (m.Method.DeclaringType == typeof(Queryable) || m.Method.DeclaringType == typeof(Enumerable)) {
                switch (m.Method.Name) {
                    case "Where":
                        return this.BindWhere(m.Type, m.Arguments[0], (LambdaExpression)StripQuotes(m.Arguments[1]));
                    case "Select":
                        return this.BindSelect(m.Type, m.Arguments[0], (LambdaExpression)StripQuotes(m.Arguments[1]));
                    case "Group":
                        break;
                    case "Count":
                        return BindCount(m.Method.GetGenericArguments()[0], m.Arguments[0]);
                    case "Skip":
                        return BindSkip(m.Arguments[0], m.Arguments[1]);
                    case "Take":
                        return BindTake(m.Arguments[0], m.Arguments[1]);
                    case "ElementAt":
                        return BindElementAt(m.Arguments[0], m.Arguments[1]);
                    case "First":
                        return BindElementAt(m.Arguments[0], Expression.Constant(0));
                    case "Last":
                        return BindElementAt(m.Arguments[0], Expression.Constant(0));
                }

                throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
            }

            return base.VisitMethodCall(m);
        }

        private Expression BindElementAt(Expression source, Expression elementAtExpression) {
            var projection = (ProjectionExpression)this.Visit(source);
            return new ElementAtExpression(projection, this.Visit(elementAtExpression));
        }

        private Expression BindTake(Expression source, Expression takeCount) {
            var projection = (ProjectionExpression)this.Visit(source);

            //REVIEW: not sure why i put this here with my WalkMethod stuff
            Expression skipExp = Expression.Constant(0, typeof(int));

            if (projection is SkipExpression) {
                skipExp = (projection as SkipExpression).SkipAmount;
            }

            // create an expression to convert to skip and take, wrapping the projector
            return new TakeExpression(projection, skipExp, takeCount);
        }

        private Expression BindSkip(Expression source, Expression count) {
            var projection = (ProjectionExpression)this.Visit(source);
            return new SkipExpression(projection, this.Visit(count));
        }

        private Expression BindCount(Type resultType, Expression source) {
            var projection = (ProjectionExpression)this.Visit(source);
            return new CountExpression(projection);
        }

        private Expression BindWhere(Type resultType, Expression source, LambdaExpression predicate) {
            ProjectionExpression projection = (ProjectionExpression)this.Visit(source);
            this.map[predicate.Parameters[0]] = projection.Projector;
            Expression where = this.Visit(predicate.Body);
            string alias = this.GetNextAlias();
            ProjectedFields pf = this.ProjectFields(projection.Projector, alias, GetExistingAlias(projection.Source));

            return new ProjectionExpression(
                new SelectExpression(resultType, alias, pf.Fields, null, projection.Source, where),
                pf.Projector
                );
        }

        private Expression BindSelect(Type resultType, Expression source, LambdaExpression selector) {
            ProjectionExpression projection = (ProjectionExpression)this.Visit(source);
            this.map[selector.Parameters[0]] = projection.Projector;
            Expression expression = this.Visit(selector.Body);
            string alias = this.GetNextAlias();
            ProjectedFields pf = this.ProjectFields(expression, alias, GetExistingAlias(projection.Source));

            return new ProjectionExpression(
                new SelectExpression(resultType, alias, pf.Fields, null, projection.Source, null),
                pf.Projector
                );
        }

        private static string GetExistingAlias(Expression source) {
            switch ((LuceneExpressionType)source.NodeType) {
                case LuceneExpressionType.Select:
                    return ((SelectExpression)source).Alias;
                case LuceneExpressionType.Index:
                    return ((IndexExpression)source).Alias;
                default:
                    throw new InvalidOperationException(string.Format("Invalid source node type '{0}'", source.NodeType));
            }
        }

        private bool IsIndex(object value) {
            IQueryable q = value as IQueryable;
            return q != null && q.Expression.NodeType == ExpressionType.Constant;
        }

        private string GetIndexName(object index) {
            IQueryable indexQuery = (IQueryable)index;
            return indexQuery.ElementType.GetDocumentDetails().Name;
        }

        private string[] GetDefaultFieldNames(Type documentType) {
            return (from fd in documentType.GetFieldDetails()
                    where fd.Field.IsDefault == true
                    select fd.Name).ToArray();
        }

        private ProjectionExpression GetIndexProjection(object value) {
            IQueryable index = (IQueryable)value;
            string indexAlias = this.GetNextAlias();
            string selectAlias = this.GetNextAlias();
            List<MemberBinding> bindings = new List<MemberBinding>();
            List<FieldDeclaration> fields = new List<FieldDeclaration>();
            string[] defaultFieldNames = GetDefaultFieldNames(index.ElementType).ToArray<string>();

            foreach (var fd in index.ElementType.GetFieldDetails()) {
                string fieldName = fd.Name;
                Type fieldType = fd.Property.PropertyType;
                int ordinal = fields.Count;

                bindings.Add(Expression.Bind(fd.Property, new FieldExpression(fieldType, selectAlias, fieldName, ordinal, typeCache[fd.Field.Analyzer])));
                fields.Add(new FieldDeclaration(fieldName, new FieldExpression(fieldType, indexAlias, fieldName, ordinal, typeCache[fd.Field.Analyzer])));
            }

            Expression projector = Expression.MemberInit(Expression.New(index.ElementType), bindings);
            Type resultType = typeof(IEnumerable<>).MakeGenericType(index.ElementType);

            return new ProjectionExpression(
                new SelectExpression(
                    resultType,
                    selectAlias,
                    fields,
                    defaultFieldNames,
                    new IndexExpression(resultType, indexAlias, this.GetIndexName(index)),
                    null
                    ),
                projector
                );
        }

        protected override Expression VisitConstant(ConstantExpression c) {
            if (this.IsIndex(c.Value)) {
                return GetIndexProjection(c.Value);
            }

            return c;
        }

        protected override Expression VisitParameter(ParameterExpression p) {
            Expression e;
            if (this.map.TryGetValue(p, out e)) {
                return e;
            }

            return p;
        }

        protected override Expression VisitMemberAccess(MemberExpression m) {
            Expression source = this.Visit(m.Expression);
            switch (source.NodeType) {
                case ExpressionType.MemberInit:
                    MemberInitExpression min = (MemberInitExpression)source;
                    for (int i = 0, n = min.Bindings.Count; i < n; i++) {
                        MemberAssignment assign = min.Bindings[i] as MemberAssignment;
                        if (assign != null && MembersMatch(assign.Member, m.Member)) {
                            return assign.Expression;
                        }
                    }
                    break;

                case ExpressionType.New:
                    NewExpression nex = (NewExpression)source;
                    if (nex.Members != null) {
                        for (int i = 0, n = nex.Members.Count; i < n; i++) {
                            if (MembersMatch(nex.Members[i], m.Member)) {
                                return nex.Arguments[i];
                            }
                        }
                    }
                    break;
            }

            if (source == m.Expression) {
                return m;
            }

            return MakeMemberAccess(source, m.Member);
        }

        private bool MembersMatch(MemberInfo a, MemberInfo b) {
            if (a == b) {
                return true;
            }

            if (a is MethodInfo && b is PropertyInfo) {
                return a == ((PropertyInfo)b).GetGetMethod();
            } else if (a is PropertyInfo && b is MethodInfo) {
                return ((PropertyInfo)a).GetGetMethod() == b;
            }

            return false;
        }

        private Expression MakeMemberAccess(Expression source, MemberInfo mi) {
            FieldInfo fi = mi as FieldInfo;
            if (fi != null) {
                return Expression.Field(source, fi);
            }

            PropertyInfo pi = (PropertyInfo)mi;
            return Expression.Property(source, pi);
        }
    }
}