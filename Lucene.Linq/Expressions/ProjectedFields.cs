using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Collections.ObjectModel;

namespace Lucene.Linq.Expressions
{
  internal sealed class ProjectedFields
  {
    Expression projector;
    ReadOnlyCollection<FieldDeclaration> fields;

    internal ProjectedFields(Expression projector, ReadOnlyCollection<FieldDeclaration> fields)
    {
      this.projector = projector;
      this.fields = fields;
    }

    internal Expression Projector
    {
      get { return this.projector; }
    }

    internal ReadOnlyCollection<FieldDeclaration> Fields
    {
      get { return this.fields; }
    }
  }
}
