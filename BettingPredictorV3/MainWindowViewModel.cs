using BettingPredictorV3.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingPredictorV3
{
    class MainWindowViewModel
    {
        private Database database = new Database();

        public MainWindowViewModel(Database aDatabase)
        {
            database = aDatabase;
        }

        public List<Fixture> GetDefaultUpcomingFixtures()
        {
            List<Fixture> upcomingFixtures = new List<Fixture>();
            upcomingFixtures = database.FixtureList;
            // remove teams with less than a season of results
            upcomingFixtures.RemoveAll(x => x.HomeTeam.GetFixturesBefore(DateTime.Now).Count < 19);
            upcomingFixtures.RemoveAll(x => x.AwayTeam.GetFixturesBefore(DateTime.Now).Count < 19);
            return upcomingFixtures;
        }
    }
}
