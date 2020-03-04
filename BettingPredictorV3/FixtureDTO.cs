using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingPredictorV3
{
    public class FixtureDTO
    {
        public string LeagueCode { get; internal set; }
        public DateTime Date { get; internal set; }
        public string HomeTeamName { get; internal set; }
        public string AwayTeamName { get; internal set; }
        public List<Bookmaker> Odds { get; internal set; }
    }
}
