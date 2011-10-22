using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Xml;
using System.Xml.Linq;
using dotSearch.Site.net.live.search.api;

namespace dotSearch.Site
{
    public class dotSearchResult
    {
        public string pageTitle;
        public string pageUrl;
        public string pageDescription;
        public int dotPriority;
        public Guid pageID;
        public dotSearchEngine engine;
    }

    public enum dotSearchEngine
    {
        dotSearch = 50,
        Google = 100,
        Bing = 150
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

        public static List<dotSearchResult> GoogleSearch(string search_expression)
        {
            var url_template = @"http://ajax.googleapis.com/ajax/services/search/web?v=1.0&rsz=large&safe=active&q={0}&start={1}";

            Uri search_url;
            var results_list = new List<dotSearchResult>();
            int[] offsets = { 0, 8, 16, 24, 32, 40, 48 };

            foreach (var offset in offsets)
            {
                search_url = new Uri(string.Format(url_template, search_expression, offset));
                var page = new WebClient().DownloadString(search_url);
                JObject o = (JObject)JsonConvert.DeserializeObject(page);
                int i = 1000;
                if (o != null)
                {
                    try
                    {
                        var results_query = from result in o["responseData"]["results"].Children()
                                            select new dotSearchResult
                                                {
                                                    pageUrl = result.Value<string>("url").ToString(),
                                                    pageTitle = result.Value<string>("title").ToString(),
                                                    pageDescription = result.Value<string>("content").ToString(),
                                                    engine = dotSearchEngine.Google,
                                                    dotPriority = i++
                                                };

                        foreach (var result in results_query)
                            results_list.Add(result);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            return results_list;
        }

        public static List<dotSearchResult> BingSearch(string search_expression)
        {
            List<dotSearchResult> results_list = new List<dotSearchResult>();

            using (BingService searchService = new BingService())
            {
                SearchRequest searchRequest = new SearchRequest();
                searchRequest.AppId = "36149AF47B0C19CE5FCAE52FFA14101135EA61D9";
                searchRequest.Query = search_expression;
                searchRequest.Sources = new SourceType[] { SourceType.Web };
                searchRequest.Web = new net.live.search.api.WebRequest();
                searchRequest.Web.Count = 50;
                searchRequest.Web.CountSpecified = true;
                searchRequest.Web.Offset = 0;
                searchRequest.Web.OffsetSpecified = true;

                SearchResponse searchResponse = new SearchResponse();
                searchResponse = searchService.Search(searchRequest);

                int i = 1000;

                foreach (WebResult result in searchResponse.Web.Results)
                {
                    dotSearchResult temp = new dotSearchResult();
                    temp.pageUrl = result.Url;
                    temp.pageTitle = result.Title;
                    temp.pageDescription = result.Description;
                    temp.dotPriority = i++;
                    temp.engine = dotSearchEngine.Bing;

                    results_list.Add(temp);
                }
            }

            return results_list;
        }

        public static string makeBold(string text, string keyword)
        {
            string result = text;
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                if (text.Contains(keyword))
                    result = text.Replace(keyword, "<b>" + keyword + "</b>");
            }
            return result;
        }
    }
}