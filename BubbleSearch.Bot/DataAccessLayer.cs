using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotSearch.Bot
{
    /// <summary>
    /// Méthodes d'accès à la base
    /// </summary>
    class DataAccessLayer
    {
        /// <summary>
        /// Ajout d'une page dans la bdd
        /// </summary>
        /// <param name="page">objet page à insérer en base</param>
        public static void AddPage(Page page)
        {
            dotSearchDataContext.dotBaseDataContext context = null;
            context = new dotSearchDataContext.dotBaseDataContext();

            if (page.Domain != null && (page.Occurences.Count > 0 || page.Images.Count > 0) && page.Url != null)
            {
                dotSearchDataContext.Category bddCat = (from c in context.Categories where c.name_category.Equals(page.Category) select c).FirstOrDefault();
                if (bddCat == null)
                {
                    bddCat = new dotSearchDataContext.Category();
                    bddCat.id_category = Guid.NewGuid();
                    bddCat.name_category = page.Category;
                    context.Categories.InsertOnSubmit(bddCat);
                }

                dotSearchDataContext.Site bddSite = (from s in context.Sites where s.TITLE.Equals(page.Domain) select s).FirstOrDefault();
                if (bddSite == null)
                {
                    bddSite = new dotSearchDataContext.Site();

                    bddSite.ID_SITE = Guid.NewGuid();
                    bddSite.TITLE = page.Domain;
                    bddSite.URL_SITE = page.Protocol + page.Domain;
                    bddSite.Category = bddCat;
                    context.Sites.InsertOnSubmit(bddSite);
                }
                dotSearchDataContext.Page bddPage = (from p in context.Pages where p.url_page.Equals(page.Url) select p).FirstOrDefault();
                if (bddPage == null)
                {
                    bddPage = new dotSearchDataContext.Page();
                    bddPage.id_page = Guid.NewGuid();
                    bddPage.id_site_parent = bddSite.ID_SITE;
                    bddPage.title_page = page.Name;
                    bddPage.url_page = page.Url;

                    context.Pages.InsertOnSubmit(bddPage);
                }



                foreach (KeyValuePair<string, int> pair in page.Occurences)
                {

                    dotSearchDataContext.Word word = (from w in context.Words where w.txt_word.Equals(pair.Key) select w).FirstOrDefault();
                    if (word == null)
                    {
                        word = new dotSearchDataContext.Word();
                        word.id_word = Guid.NewGuid();
                        word.txt_word = pair.Key;
                        context.Words.InsertOnSubmit(word);
                    }


                    dotSearchDataContext.Occurrence occur = (from o in context.Occurrences where (o.id_page.Equals(bddPage.id_page) && o.id_word.Equals(word.id_word)) select o).FirstOrDefault();

                    if (occur == null)
                    {
                        occur = new dotSearchDataContext.Occurrence();
                        occur.id_occur = Guid.NewGuid();
                        occur.id_word = word.id_word;
                        occur.id_page = bddPage.id_page;
                        occur.nb_occur = pair.Value;
                        context.Occurrences.InsertOnSubmit(occur);
                    }

                }
                try
                {
                    context.SubmitChanges();
                }
                catch (Exception e) { }
                foreach (Image img in page.Images)
                {

                    dotSearchDataContext.Resource resx = (from s in context.Resources where s.url_resx.Equals(img.Src) select s).FirstOrDefault();
                    if (resx == null)
                    {
                        resx = new dotSearchDataContext.Resource();
                        resx.id_resource = Guid.NewGuid();
                        resx.name_resx = img.Description;
                        resx.url_resx = img.Src;
                        resx.id_page = bddPage.id_page;
                        resx.type_resx = "image";
                        context.Resources.InsertOnSubmit(resx);
                    }

                }
                try
                {
                    context.SubmitChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
}
