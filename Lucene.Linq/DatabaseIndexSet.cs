using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Data.SqlClient;
using System.Reflection;
using Lucene.Linq.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Analysis;

using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Lucene.Linq.Expressions;
using Lucene.Net.Store;
using System.Threading;
using System.Data.Linq;
using System.Linq.Expressions;

using System.Collections;

namespace Lucene.Linq {
    /// <summary>Index Set for a LINQ to SQL DataContext</summary>
    /// <typeparam name="TDataContext">The DataContext type</typeparam>
    public class DatabaseIndexSet<TDataContext>: IndexSet
        where TDataContext : DataContext, new() {
        #region Fields/Properties

        readonly TDataContext _dataContext;

        public TDataContext DataContext
        {
            get
            {
                return _dataContext;
            }
        }

        #endregion

        #region Ctors

        /// <summary>Creates RAM Indexes from data context</summary>
        /// <param name="dataContext">data context instance</param>
        public DatabaseIndexSet(TDataContext dataContext)
            : base() {
            _dataContext = dataContext ?? new TDataContext();
            Init();
        }

        ///<summary>Creates File System Indexes from data context</summary>
        ///<param name="path">File System path</param>
        ///<param name="dataContext">data context instance</param>
        public DatabaseIndexSet(string path, TDataContext dataContext):base(path) {
            _dataContext = dataContext ?? new TDataContext();
            Init();
        }

        ///<summary>Creates file system indexes from data context</summary>
        ///<param name="directory">File system directory info</param>
        ///<param name="dataContext">data context instance</param>
        public DatabaseIndexSet(DirectoryInfo directory, TDataContext dataContext)
            : base(directory) {
            _dataContext = dataContext ?? new TDataContext();
            Init();
        }



        private void Init() {


            Type dcType = typeof(TDataContext);

            var linqTableTypes = GetTableTypes();

            if (linqTableTypes.Count() > 0) {

                foreach (var linqTableType in linqTableTypes) {
                    try {
                        Add(linqTableType);
                    } catch (ArgumentException) {
                    }
                }
            }

        }

        #endregion
        
        #region Private Methods

        private IEnumerable<Type> GetTableTypes() {

            Type iTableType = typeof(ITable);
            var linqTableTypes = from prop in typeof(TDataContext).GetProperties()
                                 where prop.PropertyType.IsGenericType &&
                                       iTableType.IsAssignableFrom(prop.PropertyType)
                                 select prop.PropertyType.GetGenericArguments()[0];

            return linqTableTypes;
        }

        private int GetTableRecordCount(ITable table, Type tableType) {
            Expression expr = Expression.Call(typeof(Queryable), "Count", new Type[] { tableType }, Expression.Constant(table));
            int count = table.Provider.Execute<int>(expr);
            return count;
            
        }

        private IEnumerable GetAllFromTable(ITable table,Type tableType) {

            ParameterExpression param = Expression.Parameter(tableType, "c");
            Expression selector = Expression.Parameter(tableType,"c");
            Expression pred = Expression.Lambda(param, param);

            IQueryable queryable = table;
            
            Expression expr = Expression.Call(typeof(Queryable), "Select", new Type[] {tableType,tableType} , Expression.Constant(table), pred);
            IQueryable query = table.Provider.CreateQuery(expr);

            
            System.Data.Common.DbCommand cmd = _dataContext.GetCommand(query);
            Console.WriteLine("Generated T-SQL:");
            Console.WriteLine(cmd.CommandText);
            Console.WriteLine();

            return _dataContext.ExecuteQuery(tableType, cmd.CommandText);
        }

        #endregion

        #region Public Methods

        /// <summary>Write all the records from the table type into their respective indexes</summary>
        /// <typeparam name="TTable">Table type to index</typeparam>
        public void Write<TTable>() {
            Write(typeof(TTable));
        }

        /// <summary>Write all the records from the table type into their respective indexes</summary>
        /// <param name="tableType">Table type to index</param>
        public void Write(Type tableType) {

            IIndex index = this.Get(tableType);
            string name = tableType.Name;

            // Get the LINQ to SQL ITable instance
            ITable table = _dataContext.GetTable(tableType);
            if (table == null)
                throw new ArgumentException("tableType doesnt belong to db");

            // get the number of records in the table
            int itemCount = GetTableRecordCount(table, tableType);

            // get all the records from the table
            var items = GetAllFromTable(table, tableType);

            Console.WriteLine("About to write " + name + "s...");


            if (itemCount == index.Count) {
                Console.WriteLine("Not adding " + name + "s, because index is up to date.");
            } else {
                index.Add(items);
                Console.WriteLine("Added " + index.Count + " " + name + "s.");
            }
        }

        /// <summary>Write all the records from each table into their respective indexes</summary>
        public void Write() {
            var tableTypes = GetTableTypes();
            if (tableTypes.Count() > 0) {
                foreach (var tableType in tableTypes) {
                    try {
                        Write(tableType);
                    } catch (ArgumentException) {
                        // skip this table type
                    }
                }
            }


        }

        #endregion

    }
}
