//====================================================================
// Author: Jason Plante
// Date Created: 24 October 2007
// LINQ to Lucene: Copyright (c) 2007.  All rights reserved.
//====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Index;
using Lucene.Net.Analysis;

namespace Lucene.Linq.Mapping {
    /// <summary>
    /// Attribute used to Map a class to an Index
    /// Describes the way the Document will be written to and read-from the Index
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class DocumentAttribute : Attribute {
        
        #region Properties

        /// <summary>
        /// The default Analyzer.
        /// An analyzer is one that analyzes the way the text will be tokenized and written to the index for this document-type
        /// </summary>
        public Type DefaultAnalyzer { get; set; }

        /// <summary>
        /// The type that holds the field attributes.
        /// </summary>
        public Type MetadataType { get; set; }

        /// <summary>
        /// The name of the Index representing the unique documents for a particular index
        /// </summary>
        public string Name { get; set; }

        #endregion

        #region Constructors

        // REVIEW: what constructors signatures are good for this class?

        #endregion
    }
}
