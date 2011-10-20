using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace dotSearch.Bot
{
    class UrlHelper
    {
        /// <summary> 
        /// Création d'une adresse en fonction du type de l'adresse (relative/absolue)
        /// et de l'adresse de la page sur laquelle se trouve l'adresse.
        /// </summary>
        /// <param name="pageAdress">URL de la page contenant l'adresse</param>
        /// <param name="adress">Adresse concernée voulant etre résolue</param>
        /// <returns></returns>
        public static string ComposeUrl(string pageAdress, string pageDomain, string adress)
        {
            
            if (adress.Contains(pageDomain))//adresse absolue ou externe d'une ressource
            {
                return adress;
            }
            else //sinon adresse relative
            {
                Uri BaseAdress = new Uri(pageAdress);
                Uri URIComposed = new Uri("http://uri.com");
                if (Uri.TryCreate(BaseAdress, adress, out URIComposed))
                {
                    return URIComposed.ToString();
                }
                else return null;
                
            }

        }

        public static string GetURLWithoutFileName(string url)
        {
            string UrlWithoutFileName = "";
            string[] UrlArray = url.Split('/');

            for (int i = 0; i < UrlArray.Length-1; i++)
            {
                UrlWithoutFileName = UrlWithoutFileName + UrlArray[i] + "/";
            }
            return UrlWithoutFileName;
        }

        /// <summary>
        /// Helper to remove all spaces on a given string 
        /// </summary>
        /// <param name="url">string to clean</param>
        /// <returns>cleaned string</returns>
        public static string DeleteSpaces (string url)
        {
           return url.Replace(" ","") ;
        }
    }
}
