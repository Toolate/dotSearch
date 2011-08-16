//====================================================================
// Author: Jason Plante
// Date Created: 24 October 2007
// LINQ to Lucene: Copyright (c) 2007.  All rights reserved.
//====================================================================

namespace Lucene.Linq.Mapping
{
  /// <summary>
  /// Specifies whether and how a field should be stored.
  /// </summary>
  public enum FieldStore
  {
    /// <summary>
    /// Store the original field value in the index in a compressed form. This is
    /// useful for long documents and for binary valued fields.
    /// </summary>
    Compress,

    /// <summary>
    /// Store the original field value in the index. This is useful for short texts
    /// like a document's title which should be displayed with the results. The
    /// value is stored in its original form, i.e. no analyzer is used before it is
    /// stored. 
    /// </summary>
    Yes,

    /// <summary>
    /// Do not store the field value in the index. 
    /// </summary>
    No
  }
}