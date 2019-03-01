using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BettingPredictorV3
{
    [Serializable]
    public class Referee
    {
        private String name;

        public Referee(String aName)
        {
            this.Name = aName;
        }

        public string Name { get => name; set => name = value; }

        public override string ToString()
        {
            return Name;
        }
    }
}
