using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace dotSearch.Site
{
    public class dotSearchResult
    {
        public string pageTitle;
        public string pageUrl;
        public string pageDescription;
        public int dotPriority;
        public Guid pageID;
    }

    public class dotHelper
    {
        public static List<string> splitQuery(string query)
        {
            List<string> wordsList = new List<string>();
            
            string pattern = @"(-""[^""]+""|""[^""]+""|-\w+|\w+)\s*"; 

            MatchCollection mc = Regex.Matches(query, pattern);
            foreach (Match m in mc)
            {
                wordsList.Add(m.Groups[0].Value.Trim());
            }

            return wordsList;
        }
    }
}