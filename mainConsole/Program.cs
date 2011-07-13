using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;
using dotSearch.Bot;
using System.Diagnostics;

namespace mainConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() < 2)
            {
                Console.WriteLine("Erreur: Argument 1: url, Argument 2: profondeur de parcours");
                
            }
            
            BotPage botPage = new BotPage(args[0].Normalize(), System.Int32.Parse(args[1]));                     
            List<string> Errors= new List<string>();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            botPage.Run();

            sw.Stop();
            foreach (string e in botPage.Errors)
            {
                Console.WriteLine("Error: {0}", e);
            }
            Console.WriteLine("\n\nfin {0}", sw.ElapsedMilliseconds);
            Console.Beep(440, 200);
            Console.WriteLine("Niveau : {0} Nb de lien interne : {1}\nOccurences:", botPage.Depth, botPage.BotLinks.Count, botPage.Pages.Count);
            int somme = 0;
            foreach (Page p in botPage.Pages)
            {
                somme += p.Occurences.Count;
            }
            Console.WriteLine("occurences:"+somme);
            foreach (Link l in botPage.BotLinks)
            {
                Console.WriteLine(l.Url);
            }
            Console.Read();
        }   
    }
}
