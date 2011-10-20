using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Web;


namespace dotSearch.Bot
{
    /// <summary>
    /// Objet Lien (url et booleen de traitement)
    /// </summary>
    public class Link
    {
        public string Url { get; set; }
        public bool Treated { get; set; }

        public Link(string UrlToAdd, bool isTreated)
        {
            Url = UrlToAdd;
            Treated = false;
        }
    }
    /// <summary>
    /// Robot crawler
    /// </summary>
    public class BotPage
    {
        public int Depth
        {
            get;
            set;
        }
        public List<Page> Pages { get; set; }
        public List<Link> BotLinks { get; set; }
        public List<string> Errors { get; set; }


        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="UrlStartPage">Url de la page de depart du robot</param>
        /// <param name="Depth">limite de page à parcourir</param>
        public BotPage(string UrlStartPage, int Depth)
        {

            this.Depth = Depth;
            Pages = new List<Page>();
            Errors = new List<string>();
            BotLinks = new List<Link>();
            //Ajout de la premiere page du robot:
            string PageContent = LoadPage(UrlStartPage);
            if (PageContent != null)
            {
                Parcours(UrlStartPage.ToLower(), Depth);
            }
        }

        /// <summary>
        /// Fonction chargeant le contenu d'une page web en mémoire
        /// </summary>
        /// <param name="url">url de la page à parcourir</param>
        /// <returns>contenu html</returns>
        public string LoadPage(string url)
        {
            try
            {
                WebClient browser = new WebClient();
                browser.Proxy.Credentials = new NetworkCredential("avestri", "biztalk2011");
                //browser.Encoding = Encoding.UTF8;      
                Encoding enc = Encoding.GetEncoding("UTF-8");
                Byte[] PageByte = browser.DownloadData(url);
                string webpage = enc.GetString(PageByte).ToLower();

                if (webpage.Contains("charset=iso-8859-1"))
                {
                    enc = Encoding.GetEncoding("iso-8859-1");
                    webpage = enc.GetString(PageByte).ToLower();
                }
                return webpage;
            }
            catch (Exception e)
            {
                Errors.Add(e.Message.ToString());
                return null;
            }

        }

        /// <summary>
        /// Methode de log des erreurs
        /// </summary>
        /// <param name="id">id de l'erreur</param>
        /// <param name="erreur">contenu de l'erreur</param>
        public void ErrorLog(string id, string erreur)
        {
            string myPath = @"./Logs";
            System.IO.Directory.CreateDirectory(myPath);
            string FileName = System.IO.Path.GetRandomFileName();
            myPath = System.IO.Path.Combine(myPath, FileName);
            if (!System.IO.File.Exists(myPath))
            {

            }
        }

        /// <summary>
        /// Pour une page donnée, parse la page recupere son contenu et ajoute la page à la liste Pages
        /// </summary>
        /// <param name="Url">Url de la page</param>
        /// <param name="Depth">Profondeur de parcours</param>
        public void Parcours(string Url, int Depth)
        {
            if (Depth > 0)
            {
                string PageContent = LoadPage(Url);
                if (PageContent != null)
                {
                    Page pageTemp = new Page(Url, PageContent);
                    pageTemp.ParseInfos();
                    pageTemp.ParseImages();
                    pageTemp.ParseLinks();
                    pageTemp.ParseKeywords();

                    Pages.Add(pageTemp);

                }
            }
        }

        /// <summary>
        /// Indexation des pages et rebouclage sur les pages suivantes
        /// </summary>
        /// <returns>Liste de string d'erreurs</returns>
        public List<string> Run()
        {
            if (Pages.Count > 0)
            {
                int CurrentDepth = Depth;
                for (int i = 0; i < Pages.Count; i++) //Pour chaque page on recupere les informations qu'elle contient, description;liens;images;
                {
                    CurrentDepth--;
                    //insertion de la page en base
                    DataAccessLayer.AddPage(Pages[i]);
                    for (int j = 0; j < Pages[i].InternalLinks.Count; j++)
                    {
                        if (!this.BotLinks.Contains(new Link(Pages[i].InternalLinks[j], false)))
                        {
                            this.BotLinks.Add(new Link(Pages[i].InternalLinks[j], false));
                        }
                    }

                    for (int j = 0; j < Pages[i].ExternalLinks.Count; j++)
                    {
                        if (!this.BotLinks.Contains(new Link(Pages[i].ExternalLinks[j], false)))
                        {
                            this.BotLinks.Add(new Link(Pages[i].ExternalLinks[j], false));
                        }
                    }

                    Pages.RemoveAt(i);

                    for (int j = 0; j < BotLinks.Count; j++) //à chaque lien interne de la pile du bot on crée une page et on parcours les infos 
                    {
                        if (BotLinks[j].Treated == false)
                        {
                            Parcours(BotLinks[j].Url, this.Depth);
                            BotLinks[j].Treated = true;
                        }
                    }

                    if (CurrentDepth == 0) { break; }
                }

            }
            return Errors;

        }

        public List<string> Stop()
        {
            Depth = 0;
            return Errors;
        }
    }


}
