using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Lucene.Net.Search;
using Lucene.Net.Documents;
using System.Linq.Expressions;
using Lucene.Linq.Expressions;
using System.ComponentModel;

namespace Lucene.Linq.Mapping {

    /// <summary>
    /// Enumerable hits reader that converts 
    /// </summary>
    /// <typeparam name="T">The return type from the enumerator</typeparam>
    internal class ProjectionReader<T> : IEnumerable<T> {
        IEnumerator<T> enumerator;

        #region Ctors

        internal ProjectionReader(Hits hits, int elementAt, Func<DocumentProjection, T> projector, IQueryProvider provider) {
            this.enumerator = new SingleEnumerator(hits, elementAt, projector, provider);
        }

        internal ProjectionReader(Hits hits, int startIndex, int readCount, Func<DocumentProjection, T> projector, IQueryProvider provider) {
            this.enumerator = new Enumerator(hits, startIndex, readCount, projector, provider);
        }

        #endregion

        public IEnumerator<T> GetEnumerator() {
            IEnumerator<T> e = this.enumerator;

            if (e == null) {
                throw new InvalidOperationException("Cannot enumerate more than once");
            }

            this.enumerator = null;
            return e;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        abstract class BaseProjection: DocumentProjection {

            protected readonly Hits hits;
            protected readonly Func<DocumentProjection, T> projector;
            protected readonly IQueryProvider provider;

            protected BaseProjection(Hits hits, Func<DocumentProjection, T> projector, IQueryProvider provider) {
                this.hits = hits;
                this.projector = projector;
                this.provider = provider;                
            }

            protected object GetValue(Document document, string fieldName, Type fieldType) {
                string value = string.Empty;
                Field field = document.GetField(fieldName);

                if (field != null)
                    value = field.StringValue();
                else
                    return null;

                if (fieldType == typeof(string))
                    return value;

                TypeConverter converter = TypeDescriptor.GetConverter(fieldType);
                if (converter.CanConvertFrom(typeof(string)))
                    return converter.ConvertFrom(value);
                else
                    return null;
            }

            private static bool CanEvaluateLocally(Expression expression) {
                if (expression.NodeType == ExpressionType.Parameter || expression.NodeType.IsIndexExpression()) {
                    return false;
                }

                return true;
            }

            public override IEnumerable<E> ExecuteSubQuery<E>(LambdaExpression query) {
                var projection = (ProjectionExpression)new Replacer().Replace(query.Body, query.Parameters[0], Expression.Constant(this));
                projection = (ProjectionExpression)Evaluator.PartialEval(projection, CanEvaluateLocally);
                IEnumerable<E> result = (IEnumerable<E>)this.provider.Execute(projection);
                var list = new List<E>(result);

                if (typeof(IQueryable<E>).IsAssignableFrom(query.Body.Type)) {
                    return list.AsQueryable();
                }

                return list;
            }
        }


        class SingleEnumerator: BaseProjection, IEnumerator<T> {

            readonly int documentIndex;
            readonly Document document;

            private bool isAtEnd;
            T current;

            internal SingleEnumerator(Hits hits, 
                int documentIndex, 
                Func<DocumentProjection, T> projector, 
                IQueryProvider provider)
                :base(
                hits,
                projector,
                provider) {

                this.documentIndex = documentIndex;
                document = hits.Doc(documentIndex);
                
                if (document!=null) {
                    current = projector(this);
                }

                isAtEnd = false;
            }

            public override object GetValue(string fieldName, Type fieldType) {

                return base.GetValue(document,fieldName,fieldType);
            }

            
            public void Dispose() {}

            public bool MoveNext() {
                if (isAtEnd) {
                    return true;    
                } else {
                    isAtEnd = true;
                    return false;
                }
            }

            public void Reset() {
                isAtEnd = false;
            }

            public T Current {
                get { return current; }
            }

            object IEnumerator.Current {
                get { return Current; }
            }
        }

        class Enumerator : BaseProjection, IEnumerator<T> {

            readonly int readCount;
            readonly bool useReadCount;
            readonly bool isIHit;

            T current;
            int index;
            int count = 0;
            Document currentDocument;
            
            internal Enumerator(Hits hits, 
                int startIndex, 
                int readCount, 
                Func<DocumentProjection, T> projector, 
                IQueryProvider provider):base(hits,projector,provider) {
                
                index = startIndex;
                this.readCount = readCount;

                // use the read count if it's more than 0
                useReadCount = readCount > 0;
                isIHit = typeof(IHit).IsAssignableFrom(typeof(T));
            }

            public override object GetValue(string fieldName, Type fieldType) {
                return base.GetValue(currentDocument, fieldName, fieldType);
            }

            public T Current {
                get { return this.current; }
            }

            object IEnumerator.Current {
                get { return this.current; }
            }

            public bool MoveNext() {

                if ((useReadCount == false) || (useReadCount == true && (count < readCount))) {
                    if (index < hits.Length()) {
                        this.currentDocument = hits.Doc(index);
                        this.current = this.projector(this);

                        // Add the IHit properties
                        if (this.isIHit) {
                            var hit = (this.current as IHit);
                            hit.Relevance =  hits.Score(index);
                            hit.DocumentId = hits.Id(index);
                        }

                        index++;
                        count++;
                        return true;
                    }
                }

                return false;
            }

            public void Reset() {
                count = 0;
            }

            public void Dispose() {

            }
        }
    }
}