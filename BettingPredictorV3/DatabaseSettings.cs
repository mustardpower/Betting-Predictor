using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BettingPredictorV3
{
    static class DatabaseSettings
    {
        static public List<string> bookmakersUsed { get; set; }

        static public List<string> defaultBookmakers()
        {
            List<string> bookmakers = new List<string>();
            bookmakers.Add("Bet 365"); 
            bookmakers.Add("BetWin");
            bookmakers.Add("InterWetten");
            bookmakers.Add("Ladbrokes");
            bookmakers.Add("Pinnacle Sport");
            bookmakers.Add("William Hill");
            bookmakers.Add("Stan James");
            return bookmakers;
        }
    }
}
