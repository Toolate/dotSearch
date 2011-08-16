using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Linq.Mapping;
using Lucene.Linq.Expressions;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Analysis.Standard;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.ObjectModel;
using Lucene.Net.Store;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using System.Threading;
using Token = Lucene.Net.Analysis.Token;

namespace Lucene.Linq
{
    public static class Extensions
    {
        

        #region String

        public static bool Like(this string source, string term)
        {
            return Like(source, term, 0.5);
        }

        public static bool Like(this string source, string term, double similarity)
        {
            RAMDirectory directory = new RAMDirectory();

            IndexWriter writer = new IndexWriter(directory, new StandardAnalyzer(), true);
            Document document = new Document();
            document.Add(new Field("temp", source, Field.Store.YES, Field.Index.TOKENIZED));
            writer.AddDocument(document);
            writer.Optimize();


            IndexSearcher searcher = new IndexSearcher(directory);
            Query query = new TermQuery(new Term("temp", "m*"));
            Hits hits = searcher.Search(query);

            writer.Close();
            directory.Close();
            return hits.Length() > 0;
        }

        public static bool Match(this string source, string phrase)
        {
            throw new InvalidOperationException("Operation is reserved for Lucene Query Expression's only");
        }

        public static bool Match(this string source, params string[] terms)
        {
            throw new InvalidOperationException("Operation is reserved for Lucene Query Expression's only");
        }

        public static string Boost(this string source, int factor)
        {
            return source + "^" + factor.ToString();
        }

        public static string RequireEach(this string source)
        {
            StringBuilder phrase = new StringBuilder();
            string[] terms = source.Split(' ');

            foreach (string term in terms)
            {
                phrase.Append("+").Append(term).Append(" ");
            }

            return phrase.ToString().Trim();
        }

        public static string Require(this string source)
        {
            StringBuilder phrase = new StringBuilder();
            bool multiTerms = source.Split(' ').Length > 1;

            if (multiTerms)
            {
                phrase.Append("+(");
                phrase.Append(source);
                phrase.Append(")");
            }
            else
            {
                phrase.Append("+");
                phrase.Append(source);
            }

            return phrase.ToString().Trim();
        }

        public static bool Between(this string source, string startRange, string endRange)
        {
            throw new InvalidOperationException("Operation is reserved for Lucene Query Expression's only");
        }

        public static bool Include(this string source, string startRange, string endRange)
        {
            throw new InvalidOperationException("Operation is reserved for Lucene Query Expression's only");
        }

        #endregion

        #region IIndex

        public static bool Like(this IIndexable source, string term)
        {
            throw new InvalidOperationException("Operation is reserved for Lucene Query Expression's only");
        }

        public static bool Like(this IIndexable source, string term, double similarity)
        {
            throw new InvalidOperationException("Operation is reserved for Lucene Query Expression's only");
        }

        public static bool Match(this IIndexable source, string phrase)
        {
            throw new InvalidOperationException("Operation is reserved for Lucene Query Expression's only");
        }

        public static bool Match(this IIndexable source, params string[] terms)
        {
            throw new InvalidOperationException("Operation is reserved for Lucene Query Expression's only");
        }

        public static bool StartsWith(this IIndexable source, string phrase)
        {
            throw new InvalidOperationException("Operation is reserved for Lucene Query Expression's only");
        }

        public static bool Between(this IIndexable source, string startRange, string endRange)
        {
            throw new InvalidOperationException("Operation is reserved for Lucene Query Expression's only");
        }

        public static bool Include(this IIndexable source, string startRange, string endRange)
        {
            throw new InvalidOperationException("Operation is reserved for Lucene Query Expression's only");
        }

        public static bool Search(this IIndexable source, string queryString)
        {
            throw new InvalidOperationException("Operation is reserved for Lucene Query Expression's only");
        }

        #endregion

        #region Query<T>

        #region IEnumerable<T> Search<T>

        public static IEnumerable<T> Search<T>(this Query<T> source, string queryString, string defaultFieldName)
           
        {
            string[] defaultFieldNames = { defaultFieldName };
            return source.Search(queryString, defaultFieldNames);
        }

        public static IEnumerable<T> Search<T>(this Query<T> source, string queryString, string[] defaultFieldNames)
       
        {
            ParsingQueryTranslator translator = new ParsingQueryTranslator();
            Query query = translator.Translate(queryString, defaultFieldNames);

            IndexSearcher searcher = source.Context.Searcher;
            Hits hits = searcher.Search(query);

            Type elementType = TypeSystem.GetElementType(typeof(T));

            return Activator.CreateInstance(
              typeof(ObjectReader<>).MakeGenericType(elementType),
              BindingFlags.Instance | BindingFlags.NonPublic,
              null,
              new object[] { hits },
              null
              ) as IEnumerable<T>;
        }

        public static IEnumerable<T> Search<T>(this Query<T> source, string queryString)
    
        {
            var defaultFieldNames = (from property in typeof(T).GetProperties()
                                     from fieldAttribute in property.GetCustomAttributes(typeof(FieldAttribute), false)
                                     select new { Field = fieldAttribute as FieldAttribute, Property = property }
                                    )
                                    .Where(t => t.Field.IsDefault)
                                    .Select(t => t.Field.Name ?? t.Property.Name)
                                    .ToArray<string>();

            return source.Search(queryString, defaultFieldNames);
        }

        #endregion

        #endregion

        #region ReaderWriterLock
        



        public static IDisposable ReadLock(this ReaderWriterLock l)
        {
            l.AcquireReaderLock(-1);
            return new ActionDisposable(l.ReleaseReaderLock);
        }

        public static IDisposable ReadLock(this ReaderWriterLockSlim l)
        {
            l.EnterReadLock();
            return new ActionDisposable(l.ExitReadLock);
        }

        public static IDisposable UpgradableReadLock(this ReaderWriterLockSlim l)
        {
            l.EnterUpgradeableReadLock();
            return new ActionDisposable(l.ExitUpgradeableReadLock);
        }

        public static IDisposable WriteLock(this ReaderWriterLock l)
        {
            l.AcquireWriterLock(-1);
            return new ActionDisposable(l.ReleaseWriterLock);
        }

        public static IDisposable WriteLock(this ReaderWriterLockSlim l)
        {
            l.EnterWriteLock();
            return new ActionDisposable(l.ExitWriteLock);
        }


        #endregion

        #region Functional

        internal static Func<A, R> Memoize<A, R>(this Func<A, R> f)
        {
            var map = new Dictionary<A, R>();
            return a =>
            {
                R value;
                if (map.TryGetValue(a, out value))
                    return value;
                value = f(a);
                map.Add(a, value);
                return value;
            };
        }

        #endregion


        #region Tokenization and Analyis

        public static string[] Tokenize(this string humanInput, string fieldName, Analyzer a) {

            string filteredText = humanInput;//(filter) ? humanInput.Replace('&', ' ').Replace(',', ' ').Trim().ToLower() : humanInput;

            List<string> terms = new List<string>();
            using (var sr = new System.IO.StringReader(filteredText)) {
                TokenStream ts = a.TokenStream(fieldName, sr);
                Token inputtoken = ts.Next();
                while (inputtoken != null) {
                    string termText = inputtoken.TermText();
                    terms.Add(termText);
                    inputtoken = ts.Next();
                }
            }

            return terms.ToArray();
        }

        #endregion

        #region Random


        ///<summary>
        /// Returns random string of given length
        ///</summary>
        ///<param name="r">random this</param>
        ///<param name="size">string length</param>
        ///<returns>random string</returns>
        public static string String(this Random r, int size) {

            var builder = new StringBuilder();

            for (int i = 0; i < size; i++) {
                char ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * r.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        #endregion
    }
}
