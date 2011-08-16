using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;

namespace Lucene.Linq.Expressions {

    /// <summary>
    /// Type cache for Analyzers
    /// </summary>
    internal class AnalyzerTypeCache:TypeCache<Analyzer> {

        #region Singleton

        private static readonly AnalyzerTypeCache instance = new AnalyzerTypeCache();

        internal static AnalyzerTypeCache Instance {
            get {
                return instance;
            }
        }

        #endregion

        protected override Analyzer ActivateInstance(Type type) {
            if (type == null) {
                type = Defaults.FieldAnalyzerType;
            }

            return base.ActivateInstance(type);
        }

        protected override Analyzer DefaultInstance() {
            return ActivateInstance(Defaults.FieldAnalyzerType);
        }

    }
}
