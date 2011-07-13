using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Linq;
using BubbleSearchDataContext;
using System.Data;
using System.Diagnostics;

public partial class Results : System.Web.UI.Page
{
    public static Lucene.Linq.DatabaseIndexSet<BubbleBaseDataContext> index = null;

    public static bool isFirstLoaded = true;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack || isFirstLoaded)
        {
            string userQuery = HttpContext.Current.Request.QueryString.Get("searchQuery");
            searchArea.Text = userQuery;

            if (!string.IsNullOrWhiteSpace(userQuery))
            {
                var index = new Lucene.Linq.DatabaseIndexSet<BubbleBaseDataContext>(
                    @"C:\Index",
                    new BubbleBaseDataContext());
                index.Write();

                List<BubbleSearchDataContext.Page> pageList = new List<BubbleSearchDataContext.Page>();

                List<BubbleSearchDataContext.Word> queryWord = (from w in index.DataContext.Words
                                                                where w.txt_word.Contains(userQuery) || w.txt_word.Equals(userQuery)
                                                                join o in index.DataContext.Occurrences
                                                                on w.id_word equals o.id_word
                                                                orderby o.nb_occur descending
                                                                select w).ToList();

                foreach (BubbleSearchDataContext.Word item in queryWord)
                {
                    BubbleSearchDataContext.Occurrence occur = (from o in index.DataContext.Occurrences
                                                                where o.id_word.Equals(item.id_word)
                                                                select o).FirstOrDefault();

                    BubbleSearchDataContext.Page page = (from p in index.DataContext.Pages
                                                         where p.id_page.Equals(occur.id_page)
                                                         select p).FirstOrDefault();
                    if (!pageList.Contains(page))
                        pageList.Add(page);
                }

                DataTable dt = new DataTable();

                dt.Columns.Add();

                foreach (BubbleSearchDataContext.Page item in pageList)
                {
                    TableRow row = new TableRow();
                    TableCell cell = new TableCell();
                    HyperLink link = new HyperLink();
                    link.NavigateUrl = item.url_page;
                    link.Text = item.title_page;
                    cell.Controls.Add(link);
                    row.Controls.Add(cell);

                    TableRow row2 = new TableRow();
                    TableCell cell2 = new TableCell();
                    cell2.Text = item.description_page;
                    row2.Controls.Add(cell2);

                    TableRow row3 = new TableRow();
                    TableCell cell3 = new TableCell();
                    cell3.Text = item.url_page;
                    row3.Controls.Add(cell3);

                    TableRow row4 = new TableRow();
                    TableCell cell4 = new TableCell();
                    row4.Height = 1;
                    row4.CssClass = "resultSeparator";
                    cell4.BackColor = System.Drawing.Color.White;
                    cell4.CssClass = "resultSeparator";
                    row4.Controls.Add(cell4);

                    resultsView.Controls.Add(row);
                    resultsView.Controls.Add(row2);
                    resultsView.Controls.Add(row3);
                    resultsView.Controls.Add(row4);
                }

                isFirstLoaded = false;
            }
        }
    }

    protected void searchButton_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(searchArea.Text))
        {
            if (!HttpContext.Current.Request.Url.ToString().Contains("Results.aspx?searchQuery="))
                Response.Redirect(HttpContext.Current.Request.Url.ToString() + "Results.aspx?searchQuery=" + searchArea.Text);
            else
            {
                string url = HttpContext.Current.Request.Url.ToString();
                int index = url.ToLower().IndexOf("results.aspx?searchquery=");
                url = url.Substring(0, index);
                isFirstLoaded = true;
                Response.Redirect(url + "Results.aspx?searchQuery=" + searchArea.Text);
            }
        }
    }
}
