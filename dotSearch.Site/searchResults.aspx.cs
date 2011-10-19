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

    protected void Page_Load(object sender, EventArgs e)
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
                                                         join o in index.DataContext.Occurrences
                                                         on w.id_word equals o.id_word
                                                         orderby o.nb_occur descending
                                                         select w).ToList();

            foreach (dotSearchDataContext.Word item in queryWord)
            {
                dotSearchDataContext.Occurrence occur = (from o in index.DataContext.Occurrences
                                                         where o.id_word.Equals(item.id_word)
                                                         select o).FirstOrDefault();

                dotSearchDataContext.Page page = (from p in index.DataContext.Pages
                                                  where p.id_page.Equals(occur.id_page)
                                                  select p).FirstOrDefault();
                if (!pageList.Contains(page))
                    pageList.Add(page);
            }
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

            resultsNbr.Text = pageList.Count + " resultat(s) trouve(s) en " + Math.Round(seconds,2)  + "s";

            foreach (dotSearchDataContext.Page item in pageList)
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
        }
    }

    protected void SearchButton_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(SearchBox.Text))
        {
            string site = HttpContext.Current.Request.UrlReferrer.Authority;
            if (!string.IsNullOrEmpty(site))
                Response.Redirect("http://" + site + "/searchResults.aspx?searchQuery=" + SearchBox.Text);
        }
    }
}