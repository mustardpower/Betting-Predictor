using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BettingPredictorV3
{
    public class Referee
    {
        private String name;
        private int appearances;
        private int yellow_cards;
        private int red_cards;

        public Referee(String name)
        {
            this.name = name;
        }
        public override string ToString()
        {
            return name;
        }
    }
}
