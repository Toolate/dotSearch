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
using dotSearch.Site;

public partial class searchResults : System.Web.UI.Page
{
    public static Lucene.Linq.DatabaseIndexSet<dotBaseDataContext> index = null;
    public static bool firstTime = true;
    public static bool googleCheck = false;
    public static bool bingCheck = false;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (firstTime)
        {
            string userQuery = HttpContext.Current.Request.QueryString.Get("searchQuery");

            if (!string.IsNullOrWhiteSpace(userQuery))
            {
                SearchBox.Text = userQuery;
                Google_CheckBox.Checked = googleCheck;
                Bing_CheckBox.Checked = bingCheck;

                var index = new Lucene.Linq.DatabaseIndexSet<dotBaseDataContext>(
                    @"C:\Index",
                    new dotBaseDataContext());
                index.Write();                

                #region Split de la requete de l'utilisateur
                List<string> queryList = dotHelper.splitQuery(userQuery);
                #endregion


                if (queryList.Count > 0)
                {
                    #region Recherche d'elements
                    int priorite = 0;
                    long begin = DateTime.Now.Ticks;
                    //On recupere toutes les pages contenant le premier mot de la query
                    List<dotSearchResult> resultList = (from p in index.DataContext.Pages
                                                        join o in index.DataContext.Occurrences
                                                            on p.id_page equals o.id_page
                                                        join w in index.DataContext.Words
                                                            on o.id_word equals w.id_word
                                                        where w.txt_word.Contains(queryList[0]) || w.txt_word.Equals(queryList[0])
                                                        orderby o.nb_occur ascending
                                                        select new dotSearchResult
                                                        {
                                                            pageTitle = p.title_page,
                                                            pageUrl = p.url_page,
                                                            pageDescription = p.description_page,
                                                            pageID = p.id_page
                                                        }).ToList();
                    //On affecte la priorite a chaque page
                    resultList = resultList.AsEnumerable().Select(p => new dotSearchResult()
                    {
                        pageTitle = p.pageTitle,
                        pageUrl = p.pageUrl,
                        pageDescription = p.pageDescription,
                        pageID = p.pageID,
                        dotPriority = priorite++
                    }).ToList();

                    if (queryList.Count > 1)
                    {
                        //Pour les pages trouvees dans resultList on cherche les pages contenant les autres mots de la query 
                        for (int j = 1; j < queryList.Count; j++)
                        {
                            List<dotSearchResult> tempList = (from p in resultList
                                                        join o in index.DataContext.Occurrences
                                                            on p.pageID equals o.id_page
                                                        join w in index.DataContext.Words
                                                            on o.id_word equals w.id_word
                                                        where w.txt_word.Contains(queryList[j]) || w.txt_word.Equals(queryList[j])
                                                        orderby o.nb_occur ascending
                                                        select p).ToList();

                            if (tempList != null)
                            {
                                //On affecte la priorite a chaque page
                                tempList = tempList.AsEnumerable().Select(p => new dotSearchResult()
                                                    {
                                                        pageTitle = p.pageTitle,
                                                        pageUrl = p.pageUrl,
                                                        pageDescription = p.pageDescription,
                                                        pageID = p.pageID,
                                                        dotPriority = priorite++
                                                    }).ToList();

                                //On ajoute les nouveaux resultats aux resultats initiaux
                                resultList.AddRange(tempList); 
                            }
                        }
                    }

                    //Calcul du temps d'execution de requete
                    long end = DateTime.Now.Ticks;
                    long totalTime = end - begin;
                    TimeSpan ts = TimeSpan.FromTicks(totalTime);
                    double seconds = ts.TotalSeconds;

                    //On etablit la liste finale de resultats sans doublons et triee
                    List<dotSearchResult> finalResult = new List<dotSearchResult>();
                    foreach (dotSearchResult pageResult in resultList)
                    {
                        if (!finalResult.Exists(delegate(dotSearchResult p) { return p.pageID == pageResult.pageID;} ))
                        {
                            try
                            {
                                dotSearchResult match = (from p in resultList
                                                         where p.pageID.Equals(pageResult.pageID)
                                                         orderby p.dotPriority descending
                                                         select p).First();

                                finalResult.Add(match);
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }                    
                    #endregion                    

                    #region Binding a la ListView
                    DataTable dt = new DataTable();

                    dt.Columns.Add();

                    DataTable dtt = new DataTable();
                    dtt.Columns.Add("Title");
                    dtt.Columns.Add("URL");
                    dtt.Columns.Add("Description");

                    if (googleCheck)
                        finalResult.AddRange(dotHelper.GoogleSearch(userQuery));
                    if(bingCheck)
                        finalResult.AddRange(dotHelper.BingSearch(userQuery));

                    finalResult = finalResult.OrderByDescending(p => p.dotPriority).ToList();

                    if (finalResult.Count == 0)
                        resultsNbr.Text = "Aucun résultat trouvé";
                    else
                        resultsNbr.Text = finalResult.Count + " résultat(s) trouvé(s) en " + Math.Round(seconds, 3) + "s";

                    foreach (dotSearchResult item in finalResult)
                    {
                        string url = item.pageUrl;
                        string title = item.pageTitle;
                        string description = item.pageDescription;

                        string[] infoArray = { title, url, description };

                        dtt.LoadDataRow(infoArray, LoadOption.OverwriteChanges);
                    }
                    resultsList.DataSource = dtt;
                    resultsList.DataBind();

                    dtt.Dispose();
                    firstTime = false; 
                    #endregion
                }
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

    protected void Google_CheckBox_CheckedChanged(object sender, EventArgs e)
    {
        googleCheck = Google_CheckBox.Checked;
    }

    protected void Bing_CheckBox_CheckedChanged(object sender, EventArgs e)
    {
        bingCheck = Bing_CheckBox.Checked;
    }
}