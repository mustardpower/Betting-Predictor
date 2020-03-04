using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingPredictorV3
{
    public class HistoricalFixtureDTO : FixtureDTO
    {
        public int HomeGoals { get; internal set; }
        public int AwayGoals { get; internal set; }
    }
}
