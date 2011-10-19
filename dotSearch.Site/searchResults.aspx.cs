using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using dotSearchDataContext;
using System.Data;
using System.Data.Linq;
using System.Diagnostics;

public partial class searchResults : System.Web.UI.Page
{
    public static Lucene.Linq.DatabaseIndexSet<dotBaseDataContext> index = null;
    public static bool firstTime = true;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (firstTime)
        {
            string userQuery = HttpContext.Current.Request.QueryString.Get("searchQuery");

            if (!string.IsNullOrWhiteSpace(userQuery))
            {
                SearchBox.Text = userQuery;

                var index = new Lucene.Linq.DatabaseIndexSet<dotBaseDataContext>(
                    @"C:\Index",
                    new dotBaseDataContext());
                index.Write();

                List<dotSearchDataContext.Page> pageList = new List<dotSearchDataContext.Page>();
                long begin = DateTime.Now.Ticks;
                List<dotSearchDataContext.Word> queryWord = (from w in index.DataContext.Words
                                                             where w.txt_word.Contains(userQuery) || w.txt_word.Equals(userQuery)
                                                             select w).ToList();

                foreach (dotSearchDataContext.Word item in queryWord)
                {
                    List<dotSearchDataContext.Occurrence> occurences = (from o in index.DataContext.Occurrences
                                                                        where o.id_word.Equals(item.id_word)
                                                                        select o).ToList();

                    foreach (dotSearchDataContext.Occurrence occur in occurences)
                    {
                        dotSearchDataContext.Page page = (from p in index.DataContext.Pages
                                                          where p.id_page.Equals(occur.id_page)
                                                          select p).FirstOrDefault();
                        if (!pageList.Contains(page))
                            pageList.Add(page);
                    }
                }

                List<dotSearchDataContext.Page> pageList2 = (from p in pageList
                                                             join oc in index.DataContext.Occurrences
                                                             on p.id_page equals oc.id_page
                                                             orderby oc.nb_occur ascending
                                                             select p).ToList();

                pageList2.Reverse();


                long end = DateTime.Now.Ticks;
                long totalTime = end - begin;
                TimeSpan ts = TimeSpan.FromTicks(totalTime);
                double seconds = ts.TotalSeconds;

                DataTable dt = new DataTable();

                dt.Columns.Add();

                DataTable dtt = new DataTable();
                dtt.Columns.Add("Title");
                dtt.Columns.Add("URL");
                dtt.Columns.Add("Description");


                if (pageList.Count == 0)
                    resultsNbr.Text = "Aucun résultat trouvé";
                else
                    resultsNbr.Text = pageList.Count + " résultat(s) trouvé(s) en " + Math.Round(seconds, 3) + "s";

                foreach (dotSearchDataContext.Page item in pageList2)
                {
                    string url = item.url_page;
                    string title = item.title_page;
                    string description = item.description_page;

                    string[] infoArray = { title, url, description };

                    dtt.LoadDataRow(infoArray, LoadOption.OverwriteChanges);
                }
                resultsList.DataSource = dtt;
                resultsList.DataBind();

                dtt.Dispose();
                firstTime = false;
            }
        }
    }

    protected void SearchButton_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(SearchBox.Text))
        {
            firstTime = true;
            string site = HttpContext.Current.Request.UrlReferrer.Authority;
            if (!string.IsNullOrEmpty(site))
                Response.Redirect("http://" + site + "/searchResults.aspx?searchQuery=" + SearchBox.Text);
        }
    }
}