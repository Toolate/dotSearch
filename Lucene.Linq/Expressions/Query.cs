using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Linq.Expressions;
using Lucene.Net.Store;
using System.IO;
using Lucene.Linq.Mapping;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Analysis.Standard;
using System.Reflection;

namespace Lucene.Linq.Expressions
{
  public class Query<T> : IQueryable<T>, IQueryable, IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable 
   
  {
    #region Fields
    private string _name;
    private IndexContext _context;
    #endregion    

    #region Properties

    internal QueryProvider Provider { get; set;}
    internal Expression Expression { get; set; }

    public string QueryString
    {
      get { return Provider.QueryString.ToString(); }
    }

    public string Name
    {
      get
      {
        if (string.IsNullOrEmpty(_name))
        {
          Type type = typeof(T);
          DocumentAttribute document = type.GetCustomAttributes(typeof(DocumentAttribute), false)[0] as DocumentAttribute;
          _name = string.IsNullOrEmpty(document.Name) ? type.Name : document.Name;
        }
        return _name;
      }
    }

    internal IndexContext Context
    {
      get { return _context; }
    }

  
    #endregion

    #region Constructors

    internal Query(IndexContext context)
    {
      if (context == null)
      {
        throw new ArgumentNullException("context");
      }

      this._context = context;
      this.Provider = new QueryProvider(context);
      this.Expression = Expression.Constant(this);
    }




    internal Query(IndexContext context, Expression expression)
    {
      if (context == null)
      {
        throw new ArgumentNullException("context");
      }

      if (expression == null)
      {
        throw new ArgumentNullException("expression");
      }

      this._context = context;
      this.Provider = new QueryProvider(context);

      if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
      {
        throw new ArgumentOutOfRangeException("expression");
      }

      this.Expression = expression;
    }

    internal Query(QueryProvider provider)
    {
      if (provider == null)
      {
        throw new ArgumentNullException("provider");
      }

      this.Provider = provider;
      this.Expression = Expression.Constant(this);
    }

    internal Query(QueryProvider provider, Expression expression)
    {
      if (provider == null)
      {
        throw new ArgumentNullException("provider");
      }

      if (expression == null)
      {
        throw new ArgumentNullException("expression");
      }

      if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
      {
        throw new ArgumentOutOfRangeException("expression");
      }

      this.Provider = provider;
      this.Expression = expression;
    }

    #endregion

    #region Interface Members

    Expression IQueryable.Expression
    {
      get { return this.Expression; }
    }

    Type IQueryable.ElementType
    {
      get { return typeof(T); }
    }

    IQueryProvider IQueryable.Provider
    {
      get { return this.Provider; }
    }

    public IEnumerator<T> GetEnumerator()
    {
      return ((IEnumerable<T>)this.Provider.Execute(this.Expression)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable)this.Provider.Execute(this.Expression)).GetEnumerator();
    }

    #endregion

    public override string ToString()
    {
      return this.Provider.QueryString.ToString();
    }
  }
}
