using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BettingPredictorV3
{
    public class ProfitLossInterval
    {
        public string gdInterval { get; set; }
        public double profit { get; set; }
        public double profitYield { get; set; }
        public int numberOfMatches { get; set; }
        public string homeOrAway { get; set; }
        private float min;
        private float max;

        public ProfitLossInterval(string intervalName, string homeOrAway, int numberOfMatches, double profit, double profitYield)
        {
            this.gdInterval = intervalName;
            this.homeOrAway = homeOrAway;
            this.numberOfMatches = numberOfMatches;
            this.profit = profit;
            this.profitYield = profitYield;
        }

        public void setRange(float min, float max)
        {
            this.min = min;
            this.max = max;
            this.gdInterval = getName();
        }

        public string getName()
        {
            return "Min: " + min.ToString() + " Max: " + max.ToString();
        }
    }
}
