using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BettingPredictorV3
{
    public class Bookmaker
    {
        private string name;
        private double home_odds;
        private double draw_odds;
        private double away_odds;

        public Bookmaker(string name, double home,double draw,double away)
        {
            this.name = name;
            this.home_odds = home;
            this.draw_odds = draw;
            this.away_odds = away;
        }

        public double getHomeOdds()
        {
            return home_odds;
        }
        public double getDrawOdds()
        {
            return draw_odds;
        }
        public double getAwayOdds()
        {
            return away_odds;
        }

        public string getName()
        {
            return name;
        }
    }
}
