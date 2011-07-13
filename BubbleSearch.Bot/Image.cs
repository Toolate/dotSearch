using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotSearch.Bot
{
    /// <summary>
    /// Objet image
    /// src: chemin url de l'image
    /// Keywords: liste de string definissant l'image et son contenu
    /// </summary>
    public class Image
    {
        public string PageUrl { get; set; }
        public string Src { get; set; }
        public string Description {get; set;}

        public Image(string PageUrl, string Src, string Description)
        {
            this.PageUrl = PageUrl;
            this.Src = Src;
            this.Description = Description;
        }
    }
}
