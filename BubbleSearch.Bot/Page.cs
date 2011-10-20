using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack; //Html parser codeplex libre d'utilisation
using Lucene.Net.Analysis;
using System.Text.RegularExpressions;

namespace dotSearch.Bot
{

    /// <summary>
    /// Page web et son contenu
    /// </summary>
    public class Page
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Domain { get; set; }
        public string Protocol { get; set; }
        public string Category { get; set; }
        public List<string> InternalLinks { get; set; }
        public List<string> ExternalLinks { get; set; }
        public Dictionary<string, int> Occurences { get; set; }
        public List<Image> Images { get; set; }
        public HtmlDocument HtmlContent { get; set; }
        public int Rank;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="url">url complet de la page</param>
        /// <param name="PageContent">contenu html</param>
        public Page(string url, string PageContent)
        {
            this.Category = "Divers";
            this.Name = null;
            this.Url = url;
            this.Domain = GetDomain(url);
            this.Protocol = GetProtocol(url);
            InternalLinks = new List<string>();
            ExternalLinks = new List<string>();
            Images = new List<Image>();
            Occurences = new Dictionary<string, int>();
            Rank = 0;//GetRanking(GetDomain(url));
            HtmlContent = new HtmlDocument();
            PageContent = System.Net.WebUtility.HtmlDecode(PageContent);
            HtmlContent.LoadHtml(PageContent);
        }

        /// <summary>
        /// Recupere le domaine ou sous domaine du lien + le protocole
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetDomain(string url)
        {
            if (url.Contains("://"))
            {
                url = Regex.Split(url, "://")[1];
            }
            return url.Split('/')[0];

        }

        public string GetProtocol(string url)
        {
            if (url.Contains("://"))
                return Regex.Split(url, "://")[0] + "://";
            else return "";
        }
        /// <summary>
        /// Méthode recuperant les informations de la page
        /// </summary>
        public void ParseInfos()
        {
            //this.Domain =   this.Url.
            if (HtmlContent != null)
            {
                HtmlNode leNode = this.HtmlContent.DocumentNode.SelectSingleNode("/html/head/title");
                if (leNode != null)
                    this.Name = System.Net.WebUtility.HtmlDecode(leNode.InnerText);

            }
        }

        /// <summary>
        /// Methode recuperant les liens internes et externes de la page
        /// </summary>
        public void ParseLinks()
        {
            string href = null;

            if (this.HtmlContent != null)
            {
                HtmlNodeCollection aCollection = HtmlContent.DocumentNode.SelectNodes("//a");
                if (aCollection != null)
                {
                    foreach (HtmlNode a in aCollection)
                    {
                        href = a.GetAttributeValue("href", "false").ToString().Trim();
                        //classement rapide des url, liens externe en ''http://'' sinon interne
                        if (href.Length > 7)
                        {
                            if (href.Substring(0, 7) == "http://" || href.Substring(0, 8) == "https://" || href.Substring(0, 4) == "www.")
                            {
                                if (!this.ExternalLinks.Contains(href))
                                    this.ExternalLinks.Add(href);
                            }
                            else
                            {
                                if (!this.InternalLinks.Contains(Protocol + Domain + "/" + href))
                                    this.InternalLinks.Add(Protocol + Domain + "/" + href);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// recherche les images dans le contenu d'une page, insere les details dans la liste d'image : Images
        /// </summary>
        public void ParseImages()
        {
            if (this.HtmlContent != null)
            {
                //selection de toutes les balises img
                HtmlNodeCollection imgCollection = this.HtmlContent.DocumentNode.SelectNodes("//img");

                if (imgCollection != null)
                {
                    string src = null;
                    string desc = null;
                    List<string> ImageDetails = new List<string>();

                    foreach (HtmlNode img in imgCollection)
                    {                        
                        //Recuperation du contexte de l'image, description, titre:
                        src = img.GetAttributeValue("src", "test").ToString();
                        src = UrlHelper.ComposeUrl(this.Url, this.Domain, src);
                        desc = img.GetAttributeValue("alt", "").ToString() + img.GetAttributeValue("title", "").ToString();
                        desc = desc.Replace("<br/>", " ");
                        desc = desc.Trim();
                        //Insertion dans Images de son URL et des motclefs
                        if (desc != "")
                        {
                            Images.Add(new Image(Url, src, desc));
                        }

                    }
                }
            }

        }


        /// <summary>
        /// Recupere les mots clefs de la page et appel KeywordsAnalysis()
        /// </summary>
        public void ParseKeywords()
        {
            if (HtmlContent != null)//Si page non vide, on analyse le contenu textuel pour trouver les mots cles
            { 
                try
                {
                    List<HtmlNode> words = new List<HtmlNode>();
                    HtmlNodeCollection TempNodeCollection = this.HtmlContent.DocumentNode.SelectNodes(@"//p");
                    if (TempNodeCollection != null && TempNodeCollection.Count > 0)
                    {
                        words.AddRange(TempNodeCollection.ToList());
                    }
                    TempNodeCollection = this.HtmlContent.DocumentNode.SelectNodes(@"//a");
                    if (TempNodeCollection != null && TempNodeCollection.Count > 0)
                    {
                        words.AddRange(TempNodeCollection.ToList());
                    }
                    TempNodeCollection = this.HtmlContent.DocumentNode.SelectNodes(@"/html/head/title");
                    if (TempNodeCollection != null && TempNodeCollection.Count > 0)
                    {
                        words.AddRange(TempNodeCollection.ToList());
                    }
                    TempNodeCollection = this.HtmlContent.DocumentNode.SelectNodes(@"//h1");
                    if (TempNodeCollection != null && TempNodeCollection.Count > 0)
                    {
                        words.AddRange(TempNodeCollection.ToList());
                    }
                    TempNodeCollection = this.HtmlContent.DocumentNode.SelectNodes(@"//h2");
                    if (TempNodeCollection != null && TempNodeCollection.Count > 0)
                    {
                        words.AddRange(TempNodeCollection.ToList());
                    }
                    TempNodeCollection = this.HtmlContent.DocumentNode.SelectNodes(@"//h3");
                    if (TempNodeCollection != null && TempNodeCollection.Count > 0)
                    {
                        words.AddRange(TempNodeCollection.ToList());
                    }
                    TempNodeCollection = this.HtmlContent.DocumentNode.SelectNodes(@"//h4");
                    if (TempNodeCollection != null && TempNodeCollection.Count > 0)
                    {
                        words.AddRange(TempNodeCollection.ToList());
                    }
                    TempNodeCollection = this.HtmlContent.DocumentNode.SelectNodes(@"//h5");
                    if (TempNodeCollection != null && TempNodeCollection.Count > 0)
                    {
                        words.AddRange(TempNodeCollection.ToList());
                    }
                    TempNodeCollection = this.HtmlContent.DocumentNode.SelectNodes(@"//h6");
                    if (TempNodeCollection != null && TempNodeCollection.Count > 0)
                    {
                        words.AddRange(TempNodeCollection.ToList());
                    }

                    if (words != null)
                    {
                        KeywordsAnalysis(words);
                    }
                }
                catch
                {

                }

            }
        }

     /*   public void KeywordsAnalysis(List<string> collection)
        {

            //Ici travail de découpage et nettoyage de la string vers une liste de mot clé
            string replacement = " ";
            string result = null;

            Regex rgx2 = new Regex(@"\W");
            List<string> KeywordList = new List<string>();
            collection.Add(this.Name); //on additionne les infos titre de page
            foreach (string item in collection)
            {
                result = rgx2.Replace(item, replacement);
                KeywordList.AddRange(result.Split(' ').ToList());
            }

            KeywordList = (from s in KeywordList orderby s select s).ToList();
            string temp = "";
            int cpt = 0;

            foreach (string s in KeywordList)
            {
                string sCleaned = UrlHelper.DeleteSpaces(s);

                if (temp.CompareTo(sCleaned) == 0)
                {
                    cpt++;
                    Occurences[sCleaned] = cpt;
                }
                else
                {
                        
                        temp = sCleaned;
                        cpt = 1;
                        Occurences.Add(sCleaned, cpt);
                }

            }
        }

        */




        /// <summary>
        /// String analyser, word occurences counter before dictionnary insertion
                /// </summary>
        /// <param name="collection">Keywords Collection</param>
        public void KeywordsAnalysis(IEnumerable<HtmlNode> collection)
        {

            //Ici travail de découpage et nettoyage de la string vers une liste de mot clé

            string replacement = " ";
            string result = null;
            Regex rgx2 = new Regex(@"\s+");
            Regex rgx = new Regex(@"\W");
           // Regex rgx = new Regex(";.\"':$£?!/");
            //Regex rgx3 = new Regex(@" ");
            List<string> KeywordList = new List<string>();

            foreach (HtmlNode item in collection)
            {
                //result = System.Net.WebUtility.HtmlDecode(result);//Convert any HTML char in utf8 standard char
                result = rgx.Replace((item as HtmlNode).InnerText, replacement);
                result = rgx2.Replace(result, replacement);
                
                //result = rgx3.Replace(result, replacement);
                //result = item.InnerText;

                if (result != null)
                {
                    result = result.Trim();
                    string[] tabtest = result.Split(' ');
                    foreach (string s in tabtest)
                    {
                        string sCleaned = UrlHelper.DeleteSpaces(s); //Delete any spaces in the word expression
                        if (sCleaned != "" && sCleaned.Length >1)
                        {                            
                            
                            KeywordList.Add(sCleaned);
                        }
                    }
                }
            }

            KeywordList = (from s in KeywordList orderby s select s).ToList();
            string temp = "";
            int cpt = 0;

            foreach (string s in KeywordList)
            {
               
                if (temp.CompareTo(s) == 0)
                {
                    cpt++;
                    Occurences[s] = cpt;
                }
                else
                {
                    temp = s;
                    cpt = 1;
                    Occurences.Add(s, cpt);
                }
            }



        }


    }
}
