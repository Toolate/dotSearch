using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using System.IO;
using Lucene.Net.Index;
using Lucene.Linq.Mapping;

namespace Lucene.Linq.Expressions {
    internal class QueryProvider : IQueryProvider {

        #region Fields/Properties
        private IndexContext _context;
        private StringBuilder _queryString;


        internal StringBuilder QueryString {
            get {
                if (_queryString == null) {
                    _queryString = new StringBuilder();
                }

                return _queryString;
            }
        }

        #endregion

        #region Constructors

        internal QueryProvider(IndexContext context) {
            _context = context;
        }

        #endregion

        #region IQueryProvider Members

        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression) {
            return new Query<S>(this, expression);
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression) {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            try {
                return (IQueryable)Activator.CreateInstance(typeof(Query<>).MakeGenericType(elementType), new object[] { this, expression });
            } catch (TargetInvocationException tie) {
                throw tie.InnerException;
            }
        }

        S IQueryProvider.Execute<S>(Expression expression) {
            return (S)this.Execute(expression);
        }


        #endregion

        #region Instance Methods

        public object Execute(Expression expression) {
            return this.Execute(this.Translate(expression));
        }

        private object Execute(TranslateResult result) {

            // Compile the Projector
            Delegate projector = result.Projector.Compile();

            using (_context.EnterReadLock()) {
                Hits hits = _context.Searcher.Search(result.Query);

                //
                // check for count method
                //
                if (result.GetCount) {
                    if (result.Paging == null) {
                        throw new InvalidOperationException("Paging should be set");
                    }

                    if (result.Paging.TakeAmount > 0)
                        return result.Paging.TakeAmount;

                    return hits.Length() - result.Paging.StartIndex;
                }

                Type elementType = TypeSystem.GetElementType(result.Projector.Body.Type);

                // TODO: first, last

                //
                // check for single element return
                //

                bool shouldReturnOneItem = result.ElementAt.HasValue || result.GetLast;
                int elementAt = result.ElementAt ?? (hits.Length() - 1);

                // Check for Element At
                if (shouldReturnOneItem) {

                    // Create the enumerator
                    var resultEnumerator = Activator.CreateInstance(
                        typeof(ProjectionReader<>).MakeGenericType(elementType),
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new object[] {
                                         hits,
                                         elementAt,
                                         projector,
                                         this
                                     },
                        null
                        ) as IEnumerable;

                    // Check for null
                    if (resultEnumerator == null)
                        throw new Exception("result enumerator as IEnumerable is null for some reason");

                    // get the instance and return it as an object
                    return resultEnumerator.GetEnumerator().Current;
                }

                return Activator.CreateInstance(
                    typeof(ProjectionReader<>).MakeGenericType(elementType),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new object[] { 
                                   hits, 
                                   result.Paging.StartIndex, 
                                   result.Paging.TakeAmount, 
                                   projector, 
                                   this 
                    },
                    null
                    );
            }
        }

        internal class TranslateResult {
            internal Query Query = null;
            internal LambdaExpression Projector = null;
            internal bool GetCount = false;
            internal int? ElementAt = null;
            internal ResultPaging Paging = null;
            internal IndexSearcher Searcher = null;
            internal bool GetLast = false;
        }

        internal class ResultPaging {
            internal int StartIndex = 0;
            internal int TakeAmount = 0;
        }



        private TranslateResult Translate(Expression expression) {
            _queryString = new StringBuilder();
            var projection = expression as ProjectionExpression;

            if (projection == null) {
                expression = Evaluator.PartialEval(expression);
                projection = (ProjectionExpression)new QueryBinder().Bind(expression);
            }

            int startIndex = 0;
            int takeAmount = 0;
            int? elementAt = null;
            bool getLast = false;

            // REVIEW: is this walk methods algorithm flawed? I'm using specific Expression Types if needed, what's the right way to do this?
            // Skip needs to take into account all the skip calls chained together, if this approach is wrong, how do I accomplish this?

            startIndex = WalkMethods<int>(expression, new Predicate<MethodCallExpression>((e) => e.Method.Name == "Skip")).Sum();
            takeAmount = WalkMethods<int>(expression, new Predicate<MethodCallExpression>((e) => e.Method.Name == "Take")).LastOrDefault();
            try {
                elementAt = WalkMethods<int>(expression, new Predicate<MethodCallExpression>((e) => e.Method.Name == "ElementAt")).Last();
            } catch (Exception) {
                elementAt = null;
            }
            // first and last
            if (CountMethods(expression, new Predicate<MethodCallExpression>((e) => e.Method.Name == "First")) >0 ) {
                elementAt = 0;    
            }
            getLast = CountMethods(expression, new Predicate<MethodCallExpression>((e) => e.Method.Name == "Last")) > 0;

            // translate the expression into the lucene query
            // TODO: support switchable translators
            Query query = new ParsingQueryTranslator(QueryString).Translate(projection.Source);

            // build the real projector
            LambdaExpression projector = new ProjectionBuilder().Build(projection.Projector, projection.Source.Alias);

            return new TranslateResult {
                Query = query,
                Projector = projector,
                GetCount = projection is CountExpression,
                Paging = new ResultPaging() {
                    StartIndex = startIndex,
                    TakeAmount = takeAmount
                },
                ElementAt = elementAt
                                           ,
                GetLast = getLast
            };
        }

        private IEnumerable<T> WalkMethods<T>(Expression exp, Predicate<MethodCallExpression> pred) {
            if (exp == null) {
                throw new ArgumentNullException("exp");
            }
            if (pred == null) {
                throw new ArgumentNullException("pred");
            }

            Expression current = exp;
            while (current.NodeType == ExpressionType.Call) {
                var mce = (current as MethodCallExpression);

                if (pred(mce)) {
                    if (mce.Arguments[1] is ConstantExpression) {
                        yield return (T)(mce.Arguments[1] as ConstantExpression).Value;
                    }

                }
                current = mce.Arguments[0];
            }
        }

        private int CountMethods(Expression exp, Predicate<MethodCallExpression> pred) {
            if (exp == null) {
                throw new ArgumentNullException("exp");
            }
            if (pred == null) {
                throw new ArgumentNullException("pred");
            }

            Expression current = exp;
            int count = 0;
            while (current.NodeType == ExpressionType.Call) {
                var mce = (current as MethodCallExpression);

                if (pred(mce)) {
                    count++;

                }
                current = mce.Arguments[0];
            }
            return count;
        }

        #endregion
    }
}