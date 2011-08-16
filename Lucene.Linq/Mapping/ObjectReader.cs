using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data.Common;
using System.Reflection;
using Lucene.Net.Search;
using Lucene.Net.Documents;
using Lucene.Linq.Mapping;

namespace Lucene.Linq.Mapping
{
  internal class ObjectReader<T> : IEnumerable<T>, IEnumerable
  {
    Enumerator enumerator;

    internal ObjectReader(Hits hits)
    {
      this.enumerator = new Enumerator(hits);
    }

    public IEnumerator<T> GetEnumerator()
    {
      Enumerator e = this.enumerator;

      if (e == null)
      {
        throw new InvalidOperationException("Cannot enumerate more than once");
      }

      this.enumerator = null;
      return e;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }

    class Enumerator : IEnumerator<T>, IEnumerator, IDisposable
    {
      Hits hits;
      T current;
      int index = 0;

      internal Enumerator(Hits hits)
      {
        this.hits = hits;
      }

      public T Current
      {
        get { return this.current; }
      }

      object IEnumerator.Current
      {
        get { return this.current; }
      }

      public bool MoveNext()
      {
        if (index < hits.Length())
        {
          Type type = typeof(T);
          T instance = default(T);//TODO: change to activation
          Document document = hits.Doc(index);

          var properties = from property in type.GetProperties()
                           from fieldAttribute in property.GetCustomAttributes(typeof(FieldAttribute), false)
                           select new { Property = property, FieldAttribute = fieldAttribute as FieldAttribute };

          foreach (var p in properties)
          {
            if (p.FieldAttribute.Store != Field.Store.NO)
            {
              string fieldName = p.FieldAttribute.Name ?? p.Property.Name;
              p.Property.SetValue(instance, document.GetField(fieldName).StringValue(), null);
            }
          }

          this.index++;
          this.current = instance;
          return true;
        }

        return false;
      }

      public void Reset()
      {
      }

      public void Dispose()
      {
        
      }
    }
  }
}