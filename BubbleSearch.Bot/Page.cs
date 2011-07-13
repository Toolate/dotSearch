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
            HtmlContent.LoadHtml(PageContent);

        }

        /// <summary>
        /// Recupere le domaine ou sous domaine du lien + le protocole
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetDomain(string url)
        {

            if (url.Substring(0, 7).CompareTo("http://") == 0)
                url = url.Remove(0, 7);
            else if (url.Substring(0, 8).CompareTo("https://") == 0)
                url = url.Remove(0, 8);
            if (url.Substring(0, 4).CompareTo("www.") == 0)
                url = url.Remove(0, 4);

            return url.Split('/')[0];

        }

        public string GetProtocol(string url)
        {
            if (url.Substring(0, 8).CompareTo("https://") == 0)
                return "https://";
            else return "http://"; //autre protocole volontairement ignoré

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
                    this.Name = leNode.InnerText;

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
                        src = this.Protocol + this.Domain + img.GetAttributeValue("src", "test").ToString();//PROBLEME: resolution d'adresse en ./ ou ../ : remonter l'adresse de la page
                        desc = img.GetAttributeValue("alt", "false").ToString() + img.GetAttributeValue("title", "false").ToString();

                        //Insertion dans Images de son URL et des motclefs
                        Images.Add(new Image(Url, src, desc));

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
                    List<HtmlNode> words = this.HtmlContent.DocumentNode.SelectNodes(@"//p").ToList(); //toutes les balises paragraphe
                    words.AddRange(HtmlContent.DocumentNode.SelectNodes(@"//meta").ToList());


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

        public void KeywordsAnalysis(List<string> collection)
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






        /// <summary>
        /// Analyse des strings, insere les mots clefs et leur occurence dans le dictionnaire Occurences
        /// </summary>
        /// <param name="collection">Collection de mots</param>
        public void KeywordsAnalysis(IEnumerable<HtmlNode> collection)
        {

            //Ici travail de découpage et nettoyage de la string vers une liste de mot clé

            string replacement = " ";
            string result = null;
            // Regex rgx = new Regex(@"(<[^<]*>)");
            Regex rgx2 = new Regex(@"\W");
            //Regex rgx3 = new Regex(@"&\w*;");
            List<string> KeywordList = new List<string>();

            foreach (HtmlNode item in collection)
            {
                //result = rgx.Replace((item as HtmlNode).InnerText, replacement);
                //  result = rgx3.Replace(result, replacement);
                result = rgx2.Replace((item as HtmlNode).InnerText, replacement);

                KeywordList.AddRange(result.Split(' ').ToList());
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
