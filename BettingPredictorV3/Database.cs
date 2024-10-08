using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using BettingPredictorV3.DataStructures;

namespace BettingPredictorV3
{
    [Serializable]
    public class Database
    {

        public List<League> Leagues { get; }

        public List<string> FixtureFiles {
            get
            {
                return new List<string>()
                {
                    "https://www.football-data.co.uk/fixtures.csv",
                    "https://www.football-data.co.uk/new_league_fixtures.csv"
                };
            }
        }

        private Dictionary<string, List<string>> m_HistoryFiles;
        public Dictionary<string, List<string>> HistoryFiles {
            get
            {
                if(m_HistoryFiles == null)
                    m_HistoryFiles = GetDefaultHistoricalDataURLs();
                return m_HistoryFiles;
            }
        }

        private static Dictionary<string, List<string>> GetDefaultHistoricalDataURLs()
        {
            Dictionary<string, List<string>> historyFiles = new Dictionary<string, List<string>>();
            List<string> yearCodes = new List<string> { 
                //"1415", "1516", "1617", "1718", "1819", "1920", "2021", "2122"
                "22232", "2324", "2425"
            };
            List<string> leagueCodes = new List<string>
            {
                "E0", "E1", "E2", "E3", "EC", "SC0", "SC1", "SC2", "SC3",
                "D1", "D2", "I1", "I2", "SP1", "SP2", "F1", "F2", "N1", "B1", "P1", "T1", "G1"
            };

            foreach (string leagueCode in leagueCodes)
            {
                historyFiles.Add(leagueCode, new List<string>());
                foreach (string yearCode in yearCodes)
                {
                    historyFiles[leagueCode].Add(string.Format("https://www.football-data.co.uk/mmz4281/{0}/{1}.csv", yearCode, leagueCode));
                }
            }

            // The additional leagues have a different league code in the file contents
            // than in the URL. The key is the league code in the URL, the value is the league code in the file.
            var fileToURLNameMap = new Dictionary<string, string>()
            {
                { "ARG", "Argentina" },
                { "AUT", "Austria" },
                { "BRA", "Brazil" },
                { "CHN", "China" },
                { "DNK", "Denmark"},
                { "FIN", "Finland" },
                { "IRL", "Ireland" },
                { "JPN", "Japan" },
                { "MEX", "Mexico" },
                { "NOR", "Norway" },
                { "POL", "Poland" },
                { "ROU", "Romania" },
                { "RUS", "Russia" },
                { "SWE", "Sweden" },
                { "SWZ", "Switzerland" },
                { "USA", "Usa" }
            };

            foreach (var leagueCode in fileToURLNameMap)
            {
                historyFiles.Add(leagueCode.Value, new List<string> { string.Format("https://www.football-data.co.uk/new/{0}.csv", leagueCode.Key) });
            }

            return historyFiles;
        }

        public List<Fixture> FixtureList { get; set; }

        public List<string> LeagueCodes {
            get
            {
                return Leagues.Select(x => x.LeagueID).ToList();
            }
        }

        public Database()
        {
            Leagues = new List<League>();
            FixtureList = new List<Fixture>();
        }

        public void ClearData()
        {
            Leagues.Clear();
        }

        public List<Fixture> AddFixtures(List<IDatabaseObject<Fixture>> csvFixtures)
        {
            List<Fixture> fixtures = new List<Fixture>();
            foreach (var csvFixture in csvFixtures)
            {
                Fixture fixture = csvFixture.AddToDatabase(this);
                fixtures.Add(fixture);
            }

            return fixtures;
        }

        internal List<ProfitLossInterval> CalculateProfitLossIntervals()
        {
            List<Fixture> previousFixtures = GetPreviousResults();
            const float min = -3.0f;
            const float max = 3.0f;
            const int numberOfSteps = 40;
            return CalculateProfitIntervals(previousFixtures, min, max, numberOfSteps);
        }

        public List<Fixture> FilterForChosenGD(IEnumerable<Fixture> aFixtureList, double minGD, double maxGD)
        {
            aFixtureList = aFixtureList.Where(x => x.PredictedGoalDifference > minGD);
            aFixtureList = aFixtureList.Where(x => x.PredictedGoalDifference < maxGD);

            return aFixtureList.ToList();
        }

        internal static ProfitLossInterval CalculateHomeGameProfit(List<Fixture> fixtures)
        {
            double profit = 0.0;
            int ignoredTeams = 0;

            foreach (Fixture fixture in fixtures)
            {
                if (!fixture.HasCalculatedOdds())
                {
                    ignoredTeams++;
                }
                else
                {
                    if (fixture.HomeGoals > fixture.AwayGoals)
                    {
                        profit += fixture.BestHomeOdds.HomeOdds - 1;
                    }
                    else
                    {
                        profit--;
                    }
                }
            }

            double yield = (profit / fixtures.Count) * 100.0;
            string intervalName = "Test interval name";
            return new ProfitLossInterval(intervalName, "Home", fixtures.Count - ignoredTeams, profit, yield);
        }

        internal static ProfitLossInterval CalculateAwayGameProfit(List<Fixture> fixtures)
        {
            double profit = 0.0;
            int ignoredTeams = 0;

            foreach (Fixture fixture in fixtures)
            {
                if (!fixture.HasCalculatedOdds())
                {
                    ignoredTeams++;
                }
                else
                {
                    if (fixture.HomeGoals < fixture.AwayGoals)
                    {
                        profit += (fixture.BestAwayOdds.AwayOdds - 1);
                    }
                    else
                    {
                        profit--;
                    }
                }
            }

            double yield = (profit / fixtures.Count) * 100.0;
            string intervalName = "Test interval name";
            return new ProfitLossInterval(intervalName, "Away", fixtures.Count - ignoredTeams, profit, yield);
        }

        internal List<ProfitLossInterval> CalculateProfitIntervals(List<Fixture> previousFixtures, float min, float max, int n)
        {
            float h = ((max - min) / n); // step size of each interval
            float x1 = min;
            float x2 = x1 + h;
            List<Fixture> intervalFixtures = new List<Fixture>();
            List<ProfitLossInterval> profitLossIntervals = new List<ProfitLossInterval>();
            for (int i = 0; i < n; i++)
            {
                intervalFixtures = FilterForChosenGD(previousFixtures, x1, x2);
                ProfitLossInterval homeInterval = CalculateHomeGameProfit(intervalFixtures);
                ProfitLossInterval awayInterval = CalculateAwayGameProfit(intervalFixtures);
                homeInterval.SetRange(x1, x2);
                awayInterval.SetRange(x1, x2);

                x1 = x2;
                x2 += h;

                profitLossIntervals.Add(homeInterval);
                profitLossIntervals.Add(awayInterval);
            }

            return profitLossIntervals;
        }

        public void AddTeam(Team team)
        {
            foreach (League league in Leagues)
            {
                if (league.LeagueID == team.LeagueID)
                {
                    league.AddTeam(team);
                }
            }
        }

        public List<Fixture> GetPreviousResults()
        {
            List<Fixture> fixtures = new List<Fixture>();
            foreach (League league in Leagues)
            {
                fixtures.AddRange(league.GetFixtures());
            }

            return fixtures;
        }

        public Team GetTeam(string leagueCode, string teamName)
        {
            League league = Leagues.Find(x => x.LeagueID == leagueCode);
            if(league == null){ return null; }
            return league.GetTeam(teamName);
        }

        public League GetLeague(string leagueCode)
        {
            return Leagues.Find(x => x.LeagueID == leagueCode);
        }
    }
}
