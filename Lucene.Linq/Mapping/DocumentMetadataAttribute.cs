using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Lucene.Linq.Mapping {


    [Obsolete("",true)]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class DocumentMetadataAttribute : Attribute {
        private Type _metadataClassType =null;

        public DocumentMetadataAttribute(Type metadataClassType) {
            _metadataClassType = metadataClassType;
        }

        public Type MetadataClassType {
            get {
                return _metadataClassType;
            }
        }
    }
}
