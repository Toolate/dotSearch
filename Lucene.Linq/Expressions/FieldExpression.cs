using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Lucene.Net.Analysis;

namespace Lucene.Linq.Expressions {
    internal class FieldExpression : Expression {
        readonly string alias;
        readonly string name;
        readonly int ordinal;
        readonly Analyzer analyzer;

        internal FieldExpression(Type type, string alias, string name, int ordinal, Analyzer analyzer)
            : base((ExpressionType)LuceneExpressionType.Field, type) {
            this.alias = alias;
            this.name = name;
            this.ordinal = ordinal;
            this.analyzer = analyzer;
        }

        internal Analyzer Analyzer {
            get { return analyzer; }
        }

        internal string Alias {
            get { return alias; }
        }

        internal string Name {
            get { return name; }
        }

        internal int Ordinal {
            get { return ordinal; }
        }
    }
}