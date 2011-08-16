using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lucene.Net.Analysis;
using Lucene.Net.Search;
using Lucene.Net.Analysis.Standard;

namespace Lucene.Linq.Mapping {
    public class DocumentDetails {

        #region Fields/Properties

        Analyzer _defaultAnalyzer = new SimpleAnalyzer();

        public string Name { get; set; }


        public Analyzer DefaultAnalyzer {
            get {
                return _defaultAnalyzer;
            }
        }

        public Analyzer Analyzer { get; private set; }

        #endregion

        #region Ctors

        public DocumentDetails(string name, Type defaultAnalyzerType, IEnumerable<FieldDetails> fields) {
            this.Name = name;
            SetAnalyzerType(defaultAnalyzerType, fields);
        }

        #endregion

        #region Private Methods

        Analyzer CreateAnalyzerFromType(Type t) {
            if (t == null) return null;
            return Activator.CreateInstance(t) as Analyzer;
        }

        void SetAnalyzerType(Type defaultType, IEnumerable<FieldDetails> fields) {

            if (defaultType == null) {
                defaultType = typeof(StandardAnalyzer);
            }
            
            // create default analyzer
            _defaultAnalyzer = Activator.CreateInstance(defaultType) as Analyzer;
            if (_defaultAnalyzer == null) {
                throw new ArgumentException("defaultType is not an Analyzer type");
            }

            var wrapper = new PerFieldAnalyzerWrapper(_defaultAnalyzer);
            if (fields != null) {
                foreach (var fd in fields) {
                    if (fd.Field.Analyzer!=null) {
                        var fieldAnalyzer = CreateAnalyzerFromType(fd.Field.Analyzer);
                        if (fieldAnalyzer != null) {
                            wrapper.AddAnalyzer(fd.Name, fieldAnalyzer);
                        }
                    }

                    
                }
            }
            Analyzer = wrapper;
        }

        #endregion
    }
}
