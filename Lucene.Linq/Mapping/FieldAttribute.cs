//====================================================================
// Author: Jason Plante
// Date Created: 24 October 2007
// LINQ to Lucene: Copyright (c) 2007.  All rights reserved.
//====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;

namespace Lucene.Linq.Mapping {
    /// <summary>
    /// Attribute used to Map the Property of a class to an Index
    /// Describes the way the Field will be written to the Document of an Index
    /// </summary>
    /// <remarks>Enumerations were used to define some of the constructor parameters because of the few allowable types permitted in the declaration of an attribute</remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "Using a Facade into the property type to control allowable values.  FieldStore and FieldIndex provide access to the appropriate type allowing to Attribute-based contstructor to accept an alternate type since Attributes can only accept a limited number of declarative arguments.")]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class FieldAttribute : Attribute {
        #region Fields

        private Field.Index _index = Field.Index.TOKENIZED;
        private Field.Store _store = Field.Store.NO;
        private float _boost = 1.0f;
        

        #endregion

        #region Properties

        /// <summary>
        /// The name of the Field in the Document of an Index
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The boose of the field
        /// </summary>
        public float Boost {
            get {
                return _boost;
            }
            set {
                _boost = value;
            }
        }

        /// <summary>
        /// The way the Field will be Indexed in the Document of an Index
        /// </summary>
        public Field.Index Index {
            get { return _index; }
        }

        /// <summary>
        /// The way the Field will be Stored in the Document of an Index
        /// </summary>
        public Field.Store Store {
            get { return _store; }
        }

        /// <summary>The type of analyzer type used to tokenize the field</summary>
        public Type Analyzer { set; get; }

        /// <summary>This field is a default search field</summary>
        public bool IsDefault { get; set; }

        /// <summary>This field is the key field. Only one of these per Index`1</summary>
        public bool IsKey { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create an instance of the FieldAttribute using default values
        ///  Index=Tokenized
        ///  Store=No
        /// </summary>
        public FieldAttribute() { }

        /// <summary>
        /// Creates an instance of the FieldAttribute without Storing the field
        /// </summary>
        /// <param name="index"></param>
        public FieldAttribute(FieldIndex index) {
            SetFieldIndex(index);
        }

        /// <summary>
        /// Creates an instance of the FieldAttribute for a Tokenized field
        /// </summary>
        /// <param name="store"></param>
        public FieldAttribute(FieldStore store) {
            SetFieldStore(store);
        }

        /// <summary>
        /// Creates an instance of the FieldAttribute
        /// </summary>
        /// <param name="index">The way the Field will be Indexed in the Document of an Index</param>
        /// <param name="store">The way the field will be Stored in the Document of the Index</param>
        public FieldAttribute(FieldIndex index, FieldStore store) {
            SetFieldIndex(index);
            SetFieldStore(store);
        }

        #endregion

        #region Private Instance Methods

        /// <summary>
        /// Sets the Index value based on the corresponding-Enumeration value
        /// </summary>
        /// <remarks>Enumerations were used to define the FieldIndex because of the few allowable types permitted in the declaration of an attribute</remarks>
        /// <param name="index">The index value of the field describing how the field will be indexed in the document</param>
        private void SetFieldIndex(FieldIndex index) {
            switch (index) {
                case FieldIndex.No:
                    _index = Field.Index.NO;
                    break;
                case FieldIndex.NoNorms:
                    _index = Field.Index.NO_NORMS;
                    break;
                case FieldIndex.UnTokenized:
                    _index = Field.Index.UN_TOKENIZED;
                    break;
                default:
                case FieldIndex.Tokenized:
                    _index = Field.Index.TOKENIZED;
                    break;
            }
        }

        /// <summary>
        /// Sets the Store value based on the corresponding-Enumeration value
        /// </summary>
        /// <remarks>Enumerations were used to define the FieldStore because of the few allowable types permitted in the declaration of an attribute</remarks>
        /// <param name="store">The index value of the field describing how the field will be indexed in the document</param>
        private void SetFieldStore(FieldStore store) {
            switch (store) {
                default:
                case FieldStore.No:
                    _store = Field.Store.NO;
                    break;
                case FieldStore.Yes:
                    _store = Field.Store.YES;
                    break;
                case FieldStore.Compress:
                    _store = Field.Store.COMPRESS;
                    break;
            }
        }

        #endregion
    }
}
