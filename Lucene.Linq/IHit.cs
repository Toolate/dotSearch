using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucene.Linq {

    /// <summary>
    /// Lucene Search Hit
    /// </summary>
    public interface IHit {

        /// <summary>
        /// The integer ID of the Document in the Index
        /// </summary>
        int DocumentId { get; set; }

        /// <summary>
        /// The search result relevance score
        /// </summary>
        float Relevance { get; set; }
    }
}
