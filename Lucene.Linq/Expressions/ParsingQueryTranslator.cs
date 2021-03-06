﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Lucene.Net.Analysis.Standard;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;

namespace Lucene.Linq.Expressions {
    

    internal class ParsingQueryTranslator : LuceneExpressionVisitor, IQueryTranslator {
        StringBuilder queryString = new StringBuilder();
        string[] defaultFieldNames;

        internal StringBuilder QueryString {
            get {
                if (queryString == null) {
                    queryString = new StringBuilder();
                }

                return queryString;
            }
        }

        #region Constructors

        public ParsingQueryTranslator() {
            this.queryString = new StringBuilder();
        }

        public ParsingQueryTranslator(string queryString) {
            this.queryString = new StringBuilder(queryString);
        }

        public ParsingQueryTranslator(StringBuilder queryString) {
            this.queryString = queryString;
        }

        #endregion

        #region Translate

        public Query Translate(Expression expression) {
            // visit the expression first to generate the query
            this.Visit(expression);
            return this.Translate(QueryString.ToString(), this.defaultFieldNames);
        }

        internal Query Translate(string query, string[] defaultFieldNames) {
            QueryParser parser;

            if (query.Length == 0) {
                throw new ArgumentException("The predicate of a Lucene Term can not be the empty string.");
            }

            if (defaultFieldNames.Length == 0) {
                throw new InvalidOperationException("Lucene Document-Classes must define at least one field as a default search field");
            }

            //Console.WriteLine("Lucene.Net Query for Parser : "+query);

            if (defaultFieldNames.Length == 1) {
                parser = new QueryParser(defaultFieldNames[0], new StandardAnalyzer());
            } else {
                parser = new MultiFieldQueryParser(defaultFieldNames, new StandardAnalyzer());
            }

            return parser.Parse(query);
        }

        #endregion


        #region Private Methods

        private static Expression StripQuotes(Expression e) {
            while (e.NodeType == ExpressionType.Quote) {
                e = ((UnaryExpression)e).Operand;
            }

            return e;
        }

        private void VisitStartsWith(MethodCallExpression m) {
            string phrase = string.Empty;

            if (m.Object != null && m.Object.Type == typeof(string)) {
                FieldExpression field = (FieldExpression)m.Object;
                ConstantExpression constant = (ConstantExpression)m.Arguments[0];
                phrase = (string)constant.Value;

                int indexLength = phrase.Split(' ').Length > 1 ? 2 : 0;
                QueryString.Insert(QueryString.Length - phrase.Length - indexLength, field.Name + ":(");
                QueryString.Append("*)");
            } else {
                ConstantExpression constant = (ConstantExpression)m.Arguments[1];
                phrase = (string)constant.Value;

                QueryString.Append("(");
                this.Visit(m.Arguments[1]);
                QueryString.Append("*)");
            }

            // For multi-phrase queries, remove the quotation marks that would break it
            if (phrase.Split(' ').Length > 1) {
                QueryString.Remove(QueryString.ToString().LastIndexOf(phrase) - 1, 1);              // Left Quotation Mark
                QueryString.Remove(QueryString.ToString().LastIndexOf(phrase) + phrase.Length, 1);  // Right Quation Mark
            }
        }

        #endregion

        #region Visiting Expression Tree

        protected override Expression VisitMethodCall(MethodCallExpression m) {
            if (!(m.Arguments[0] is MemberInitExpression))
                this.Visit(m.Arguments[0]);

            if (QueryString.ToString().EndsWith(")"))
                QueryString.Append(" ");

            switch (m.Method.Name) {
                case "StartsWith":
                    VisitStartsWith(m);
                    break;
                case "Like":
                    this.Visit(m.Arguments[1]);
                    QueryString.Append("~");

                    if (m.Arguments.Count == 3)
                        QueryString.Append(m.Arguments[2]);

                    break;
                case "Match":
                    this.Visit(m.Arguments[1]);
                    break;
                case "Between":
                    QueryString.Append("{");
                    this.Visit(m.Arguments[1]);
                    QueryString.Append(" TO ");
                    this.Visit(m.Arguments[2]);
                    QueryString.Append("}");
                    break;
                case "Include":
                    QueryString.Append("[");
                    this.Visit(m.Arguments[1]);
                    QueryString.Append(" TO ");
                    this.Visit(m.Arguments[2]);
                    QueryString.Append("]");
                    break;
                case "Boost":
                    QueryString.Append("^");
                    QueryString.Append(m.Arguments[1]);
                    break;
                case "Search":
                    ConstantExpression constant = (ConstantExpression)m.Arguments[1];
                    string phrase = (string)constant.Value;

                    QueryString.Append(m.Arguments[1]);
                    QueryString.Remove(QueryString.ToString().LastIndexOf(phrase) - 1, 1);              // Left Quotation Mark
                    QueryString.Remove(QueryString.ToString().LastIndexOf(phrase) + phrase.Length, 1);  // Right Quation Mark
                    break;
                default:
                    throw new NotSupportedException(string.Format("Method '{0}' has no supported translation to Lucene", m.Method.Name));
            }

            return m;
        }

        protected override Expression VisitUnary(UnaryExpression u) {
            switch (u.NodeType) {
                case ExpressionType.Not:
                    if (!QueryString.ToString().EndsWith("(") && !QueryString.ToString().EndsWith(" "))
                        QueryString.Append(" ");

                    QueryString.Append("-");
                    this.Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }

            return u;
        }

        protected override Expression VisitBinary(BinaryExpression b) {
            QueryString.Append("(");
            this.Visit(b.Left);
            switch (b.NodeType) {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    QueryString.Append(" AND ");
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    QueryString.Append(" OR ");
                    break;
                case ExpressionType.Equal:
                    break;
                case ExpressionType.Not:
                    QueryString.Append("NOT ");
                    break;
                case ExpressionType.NotEqual:
                    int index = QueryString.ToString().LastIndexOf(" ");
                    QueryString.Insert(index + 1, "-");
                    break;
                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
            }
            this.Visit(b.Right);
            QueryString.Append(")");
            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c) {
            switch (Type.GetTypeCode(c.Value.GetType())) {
                case TypeCode.String:
                    string phrase = c.Value as string;
                    if (phrase.Split(' ').Length > 1)
                        QueryString.Append("\"").Append(c.Value).Append("\"");
                    else
                        QueryString.Append(c.Value);
                    break;
                case TypeCode.Object:
                    string[] args = c.Value as string[];

                    if (args != null) {
                        QueryString.Append("(");
                        foreach (string s in args) {
                            if (s.Split(' ').Length > 1) {
                                QueryString.Append("\"");

                                int index = s.LastIndexOf("^");
                                if (index != -1) {
                                    string m = s.Insert(index, "\"");
                                    QueryString.Append(m);
                                } else {
                                    QueryString.Append(s).Append("\"");
                                }
                            } else {
                                QueryString.Append(s);
                            }

                            QueryString.Append(" ");
                        }

                        QueryString.Remove(QueryString.Length - 1, 1);
                        QueryString.Append(")");
                    }
                    break;
                case TypeCode.Boolean:
                    QueryString.Append("(");
                    QueryString.Append("\"");
                    QueryString.Append( ((bool)c.Value)? "1":"0");
                    QueryString.Append("\"");
                    QueryString.Append(")");
                    break;
                
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Single:
                case TypeCode.Double:
                    QueryString.Append("(");
                    QueryString.Append("\"");
                    //TODO: add formatter support
                    QueryString.Append(((IFormattable)c.Value).ToString());
                    QueryString.Append("\"");
                    QueryString.Append(")");
                    break;
                
                default:
                    throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                    break;
            }

            return c;
        }

        protected override Expression VisitField(FieldExpression field) {
            QueryString.Append(field.Name).Append(":");
            return field;
        }

        protected override Expression VisitSelect(SelectExpression select) {
            if (select.DefaultFieldNames != null) {
                this.defaultFieldNames = select.DefaultFieldNames;
            }

            if (select.From != null) {
                this.VisitSource(select.From);
            }

            if (select.Where != null) {
                this.Visit(select.Where);
            }

            return select;
        }

        protected override Expression VisitSource(Expression source) {
            switch ((LuceneExpressionType)source.NodeType) {
                case LuceneExpressionType.Index:
                    IndexExpression index = (IndexExpression)source;
                    //sb.Append(index.Name);
                    //sb.Append(" AS ");
                    //sb.Append(index.Alias);
                    break;
                case LuceneExpressionType.Select:
                    SelectExpression select = (SelectExpression)source;
                    //sb.Append("(");
                    this.Visit(select);
                    //sb.Append(")");
                    //sb.Append(" AS ");
                    //sb.Append(select.Alias);
                    break;
                default:
                    throw new InvalidOperationException("Select source is not valid type");
            }
            return source;
        }

        #endregion
    }
}
