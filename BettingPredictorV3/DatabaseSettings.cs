using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BettingPredictorV3
{
    static class DatabaseSettings
    {
        static private List<string> bookmakersUsed;

        static public List<string> BookmakersUsed
        {
            get
            {
                return bookmakersUsed;
            }

            set
            {
                bookmakersUsed = value;

                // new leagues do not have odds for individual bookmakers
                // so add placeholder for overall best odds
                bookmakersUsed.Add("Best Odds"); 
            }
        }

        static public List<string> DefaultBookmakers()
        {
            List<string> bookmakers = new List<string>();
            bookmakers.Add("Bet 365"); 
            bookmakers.Add("BetWin");
            bookmakers.Add("InterWetten");
            //bookmakers.Add("Ladbrokes");
            bookmakers.Add("Pinnacle Sport");
            bookmakers.Add("William Hill");
            bookmakers.Add("Victor Chandler");
            //bookmakers.Add("Stan James");
            bookmakers.Add("Best Odds");
            return bookmakers;
        }

        static public bool IgnorePlayedFixtures { get; set; }
    }
}
