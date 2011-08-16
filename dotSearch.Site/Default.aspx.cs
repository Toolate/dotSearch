using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using dotSearchDataContext;
using System.Data;

namespace dotSearch.Site
{
    public partial class Default : System.Web.UI.Page
    {
        public static Lucene.Linq.DatabaseIndexSet<dotBaseDataContext> index = null;

        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void searchButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(searchArea.Text))
            {
                string url = HttpContext.Current.Request.Url.ToString();
                int index = url.ToLower().IndexOf("default.aspx");
                url = url.Substring(0, index);
                Response.Redirect(url + "Results.aspx?searchQuery=" + searchArea.Text);
            }
        }
    }
}