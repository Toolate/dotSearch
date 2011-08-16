using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace Lucene.Linq.Mapping {
    public class FieldDetails {
        public string Name {
            get {
                if (Field == null)
                    return Property.Name;
                if (String.IsNullOrEmpty(Field.Name))
                    return Property.Name;

                return Field.Name;
            }
        }
        public PropertyInfo Property { get; set; }
        public FieldAttribute Field { get; set; }
    }
}
