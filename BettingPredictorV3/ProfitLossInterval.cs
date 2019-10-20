using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BettingPredictorV3
{
    public class ProfitLossInterval
    {
        public string GdInterval { get; set; }
        public double Profit { get; set; }
        public double ProfitYield { get; set; }
        public int NumberOfMatches { get; set; }
        public string HomeOrAway { get; set; }
        private float min;
        private float max;

        public ProfitLossInterval(string intervalName, string homeOrAway, int numberOfMatches, double profit, double profitYield)
        {
            this.GdInterval = intervalName;
            this.HomeOrAway = homeOrAway;
            this.NumberOfMatches = numberOfMatches;
            this.Profit = profit;
            this.ProfitYield = profitYield;
        }

        public void SetRange(float min, float max)
        {
            this.min = min;
            this.max = max;
            this.GdInterval = GetName();
        }

        public string GetName()
        {
            float minRounded2sf = (float)Math.Round(min * 100f) / 100f;
            float maxRounded2sf = (float)Math.Round(max * 100f) / 100f;
            return "Min: " + minRounded2sf.ToString() + " Max: " + maxRounded2sf.ToString();
        }

        public bool Includes(double value)
        {
            return value >= min && value <= max;
        }
    }
}
