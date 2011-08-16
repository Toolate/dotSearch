using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;


namespace Lucene.Linq.Mapping {
    public static class AttributeReader {

        private static DocumentAttribute GetDocumentAttribute(this ICustomAttributeProvider type) {

            var attrs = type.GetCustomAttributes(typeof (DocumentAttribute), false);
            if (attrs==null || attrs.Length==0)
                return null;

            return attrs[0] as DocumentAttribute;
        }

        public static DocumentDetails GetDocumentDetails(this Type type) {
            var dattr = GetDocumentAttribute(type);
            if (dattr == null)
                throw new ArgumentException("Type doesn't have DocumentAttribute","type");
            var dd = new DocumentDetails(dattr.Name, dattr.DefaultAnalyzer, type.GetFieldDetails());
            return dd;
        }

        public static IEnumerable<FieldDetails> GetFieldDetails(this Type type) {

            IEnumerable<FieldDetails> details;

            // check for the meta type attribute
            var documentAttribute = type.GetDocumentAttribute();
            

            if (documentAttribute.MetadataType != null) {

                var metaDetails = from p in documentAttribute.MetadataType.GetProperties()
                                  from fieldAttribute in p.GetCustomAttributes(typeof(FieldAttribute), false)
                                  select new {
                                      PropertyName = p.Name,
                                      Field = fieldAttribute as FieldAttribute
                                  };

                // get the Properties from the source type
                details = from i in metaDetails
                          select new FieldDetails {
                              Property = type.GetProperty(i.PropertyName),
                              Field = i.Field
                          };



            } else {

                // Retrieve all of the Properties of the type that are marked as fields for the index
                // These members are required to have both a FieldAttribute and ColumnAttribute for writing an index
                details = from property in type.GetProperties()
                          from fieldAttribute in property.GetCustomAttributes(typeof(FieldAttribute), false)
                          select new FieldDetails {
                              Property = property,
                              Field = fieldAttribute as FieldAttribute
                          };
            }

            return details;
        }

    }
}
