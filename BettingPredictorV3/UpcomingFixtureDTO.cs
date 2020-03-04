using BettingPredictorV3.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingPredictorV3
{
    public class UpcomingFixtureDTO : FixtureDTO
    {
        public override Fixture CreateFixture(League league, Team homeTeam, Team awayTeam)
        {
            return new Fixture(league, Date, homeTeam, awayTeam, new Referee(""), Odds);
        }
    }
}
