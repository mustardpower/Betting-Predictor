using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BettingPredictorV3
{
    [Serializable]
    public class Bookmaker
    {
        public int BookmakerId { get; set; }

        public int FixtureId { get; set; }

        public string Name { get; set; }

        public double HomeOdds { get; set; }

        public double DrawOdds { get; set; }

        public double AwayOdds { get; set; }

        public Bookmaker() { }
        public Bookmaker(string name, double home,double draw,double away)
        {
            Name = name;
            HomeOdds = home;
            DrawOdds = draw;
            AwayOdds = away;
        }
    }
}
