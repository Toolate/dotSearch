using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lucene.Linq.Expressions;
using Lucene.Net.Store;

namespace BubbleSearchDataContext
{
    public partial class BubbleSearchIndexContext : Lucene.Linq.DatabaseIndexSet<BubbleBaseDataContext>
    {
        #region Fields

        #endregion

        #region Extension Methods
        partial void OnCreated();
        #endregion

        #region Constructors

        public BubbleSearchIndexContext(BubbleBaseDataContext database)
            : base(database)
        {
            OnCreated();
        }

        public BubbleSearchIndexContext(DirectoryInfo directory, BubbleBaseDataContext database)
            : base(directory, database)
        {
            OnCreated();
        }

        public BubbleSearchIndexContext(string path, BubbleBaseDataContext database)
            : base(path, database)
        {
            OnCreated();
        }


        #endregion

        #region Properties

        public Lucene.Linq.IIndex<Page> Pages
        {
            get
            {
                return this.Get<Page>();
            }
        }

        public Lucene.Linq.IIndex<Site> Sites
        {
            get
            {
                return this.Get<Site>();
            }
        }

        public Lucene.Linq.IIndex<Category> Categories
        {
            get
            {
                return this.Get<Category>();
            }
        }

        #endregion

        #region Methods

        #endregion
    }
}
