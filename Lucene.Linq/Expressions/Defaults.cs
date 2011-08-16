using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;

namespace Lucene.Linq.Expressions {
    internal static class Defaults {

        public static Type DocumentAnalyzerType {
            get{
                return typeof(SimpleAnalyzer);
            }
        }

        public static Type FieldAnalyzerType {
            get {
                return typeof(StandardAnalyzer);
            }
        }

        

    }
}
