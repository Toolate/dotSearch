using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using dotSearchDataContext;
using System.Data;

public partial class Default : System.Web.UI.Page
{
    public static Lucene.Linq.DatabaseIndexSet<dotBaseDataContext> index = null;

    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected void searchButton_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(searchBox.Text))
        {
            string site = HttpContext.Current.Request.UrlReferrer.Authority;
            if (!string.IsNullOrEmpty(site))
                Response.Redirect("http://" + site + "/searchResults.aspx?searchQuery=" + searchBox.Text);
        }
    }
}