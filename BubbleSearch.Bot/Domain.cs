using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BubbleSearch.Bot
{
    class Domain
    {
        public string DomainName { get; set; }
        int Ranking;

        public Domain(string Domain)
        {
            DomainName = Domain;
            Ranking = 0;
        }

        public void SetRanking (int Coeff)
        {
        
        }

        public void UpdateRanking (string Domain, int MotherPageRank)
        {
            
        }

       
    }
}
