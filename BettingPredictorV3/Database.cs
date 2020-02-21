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
    public class Database : IDisposable
    {
        private List<String> fixturesFiles;
        private Dictionary<String, List<String>> historyFiles;
        private FootballResultsDbContext dbContext;
        protected bool disposed = false;

        public List<League> Leagues
        {
            get
            {
                return dbContext.Leagues.ToList();
            }
        }

        public List<String> FixtureFiles
        {
            get
            {
                return fixturesFiles;
            }
        }

        internal List<Fixture> GetUpcomingFixtures()
        {
            return dbContext.Fixtures.Where(x => x.Date >= DateTime.Today).ToList();
        }

        public Dictionary<String, List<String>> HistoryFiles
        {
            get
            {
                return historyFiles;
            }
        }

        internal List<Fixture> GetFixturesBefore(Team aTeam, DateTime date)
        {
            ResultPredictor predictor = new ResultPredictor(dbContext);
            return predictor.GetFixturesBefore(aTeam, date);
        }

        

        public List<string> LeagueCodes {
            get
            {
                return dbContext.Leagues.Select(x => x.LeagueCode).ToList();
            }
        }

        public Database()
        {
            historyFiles = new Dictionary<String, List<String>>();
            fixturesFiles = new List<String>();
            dbContext = new FootballResultsDbContext();
        }

        public void AddUpcomingFixture(string leagueCode, DateTime date, string homeTeamName, string awayTeamName, List<Bookmaker> odds)
        {
            // turning these config settings off speeds up the insertions
            bool previousConfigurationAutoDetectChanges = dbContext.Configuration.AutoDetectChangesEnabled;
            dbContext.Configuration.AutoDetectChangesEnabled = false;

            bool previousConfigurationValidateOnSave = dbContext.Configuration.ValidateOnSaveEnabled;
            dbContext.Configuration.ValidateOnSaveEnabled = false;

            League league = GetLeague(leagueCode);
            if(league == null)
            {
                League newLeague = new League(leagueCode);
                dbContext.Leagues.Add(newLeague);
                dbContext.SaveChanges();

                league = newLeague;
            }

            Team homeTeam = GetTeam(leagueCode, homeTeamName);
            if(homeTeam == null)
            {
                homeTeam = new Team(homeTeamName);
                dbContext.Teams.Add(homeTeam);
                dbContext.SaveChanges();
            }

            Team awayTeam = GetTeam(leagueCode, awayTeamName);
            if (awayTeam == null)
            {
                awayTeam = new Team(awayTeamName);
                dbContext.Teams.Add(awayTeam);
                dbContext.SaveChanges();
            }

            var fixture = GetFixture(leagueCode, homeTeam, awayTeam, date);
            if(fixture == null)
            {
                fixture = new Fixture(league, date, homeTeam, awayTeam, new Referee(""), odds);
                dbContext.Fixtures.Add(fixture);
                dbContext.SaveChanges();
            }

            // restore the config settings that were turned off to improve speed of insertions
            dbContext.Configuration.AutoDetectChangesEnabled = previousConfigurationAutoDetectChanges;
            dbContext.Configuration.ValidateOnSaveEnabled = previousConfigurationValidateOnSave;
        }

        private Fixture GetFixture(string leagueCode, Team homeTeam, Team awayTeam, DateTime date)
        {
            return dbContext.Fixtures.ToList()
                .Where(x => x.FixtureLeague.LeagueCode == leagueCode && homeTeam.TeamId == x.HomeTeamId && awayTeam.TeamId == x.AwayTeamId && x.Date == date)
                .SingleOrDefault();
        }

        public void AddFixtures(string[] fixtures)
        {
            // turning these config settings off speeds up the insertions
            bool previousConfigurationAutoDetectChanges = dbContext.Configuration.AutoDetectChangesEnabled;
            dbContext.Configuration.AutoDetectChangesEnabled = false;

            bool previousConfigurationValidateOnSave = dbContext.Configuration.ValidateOnSaveEnabled;
            dbContext.Configuration.ValidateOnSaveEnabled = false;

            FileParser parser = new FileParser();
            foreach (string fixture in fixtures)
            {
                var fixtureData = fixture.Split(new[] { ',' }, System.StringSplitOptions.None);
                string leagueCode = fixtureData[0];
                if (leagueCode.Length > 0)
                {
                    Console.WriteLine(leagueCode + ": " + fixtureData.Length);
                    Fixture newFixture = null;

                    League aLeague = GetLeague(leagueCode);
                    if (aLeague != null)
                    {
                        newFixture = parser.ParseHistoricalFixtureData(aLeague, fixtureData);
                    }
                    else
                    {
                        aLeague = new League(leagueCode);
                        newFixture = parser.ParseHistoricalFixtureData(aLeague, fixtureData);
                        dbContext.Leagues.Add(aLeague);
                        dbContext.SaveChanges();
                    }

                    // Retrieve team from database
                    Team homeTeam = dbContext.Teams.Where(x => x.Name == newFixture.HomeTeam.Name).SingleOrDefault();
                    if (homeTeam != null)
                    {
                        // if found then use database definition
                        newFixture.HomeTeam = homeTeam;
                    }
                    else
                    {
                        // if not found then need to add team to database before saving fixture
                        dbContext.Teams.Add(newFixture.HomeTeam);
                        dbContext.SaveChanges();
                    }

                    Team awayTeam = dbContext.Teams.Where(x => x.Name == newFixture.AwayTeam.Name).SingleOrDefault();
                    if (awayTeam != null)
                    {
                        // if found then use database definition
                        newFixture.AwayTeam = awayTeam;
                    }
                    else
                    {
                        // if not found then need to add team to database before saving fixture
                        dbContext.Teams.Add(newFixture.AwayTeam);
                        dbContext.SaveChanges();
                    }

                    newFixture.LeagueId = aLeague.LeagueId;

                    dbContext.Fixtures.Add(newFixture);
                }
            }
            dbContext.SaveChanges();

            // restore the config settings that were turned off to improve speed of insertions
            dbContext.Configuration.AutoDetectChangesEnabled = previousConfigurationAutoDetectChanges;
            dbContext.Configuration.ValidateOnSaveEnabled = previousConfigurationValidateOnSave;
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
            Fixture max_odds_fixture = new Fixture
            {
                BestHomeOdds = new Bookmaker("bookie", 0.0, 0.0, 0.0)
            };

            foreach (Fixture fixture in fixtures)
            {
                if (fixture.BestHomeOdds == null)
                {
                    ignoredTeams++;
                }
                else
                {
                    if (fixture.HomeGoals > fixture.AwayGoals)
                    {
                        if (max_odds_fixture.BestHomeOdds.HomeOdds < fixture.BestHomeOdds.HomeOdds)
                        {
                            max_odds_fixture = fixture;
                        }
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
            return new ProfitLossInterval(intervalName, "Home", fixtures.Count, profit, yield);
        }

        internal static ProfitLossInterval CalculateAwayGameProfit(List<Fixture> fixtures)
        {
            double profit = 0.0;
            int ignoredTeams = 0;

            foreach (Fixture fixture in fixtures)
            {
                if (fixture.BestAwayOdds == null)
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
            return new ProfitLossInterval(intervalName, "Away", fixtures.Count, profit, yield);
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

        public List<Fixture> GetPreviousResults()
        {
            List<Fixture> fixtures = new List<Fixture>();
            foreach (League league in dbContext.Leagues)
            {
                fixtures.AddRange(league.Fixtures);
            }

            return fixtures;
        }

        public Team GetTeam(String leagueCode, String teamName)
        {
            return dbContext.Teams.ToList().Find(x => x.Name == teamName);
        }

        public League GetLeague(string leagueCode)
        {
            return dbContext.Leagues.ToList().Find(x => x.LeagueCode == leagueCode);
        }

        public void PredictResults(double alpha,double beta)
        {
            // predict the results for historical fixtures - used to find profitable betting areas 
            PredictHistoricalResults(alpha, beta);
            // predict the upcoming fixtures
            PredictUpcomingFixtures(alpha, beta);
        }

        public void PredictHistoricalResults(double alpha, double beta)
        {
            foreach (League league in dbContext.Leagues)
            {
                league.PredictResults(alpha, beta);
            }
        }

        public void PredictUpcomingFixtures(double alpha, double beta)
        {
            ResultPredictor resultPredictor = new ResultPredictor(dbContext);
            foreach (Fixture fixture in GetUpcomingFixtures())
            {
                resultPredictor.PredictResult(fixture, alpha, beta);
            }
        }

        public double GetAlphaValue()
        {
            double alpha = 0.0;
            List<double> errors = new List<double>();

            foreach (Fixture fixture in GetUpcomingFixtures())
            {
                errors.Add(fixture.AverageHomeResidual);
            }

            errors.RemoveAll(x => Double.IsNaN(x));

            if (errors.Count > 0)
            {
                alpha = errors.Average();
            }
                
            return alpha;
        }

        public double GetBetaValue()
        {
            double beta = 0.0;
            List<double> errors = new List<double>();

            foreach (Fixture fixture in GetUpcomingFixtures())
            {
                errors.Add(fixture.AverageAwayResidual);
            }

            errors.RemoveAll(x => Double.IsNaN(x));

            if (errors.Count > 0)
            {
                beta = errors.Average();
            }

            return beta;
        }

        public List<double> GetHomeResiduals(DateTime date)
        {
            List<double> residuals = new List<double>();
            foreach (League league in dbContext.Leagues)
            {
                residuals.AddRange(league.GetHomeResiduals(date));
            }

            return residuals;
        }

        public List<double> GetAwayResiduals(DateTime date)
        {
            List<double> residuals = new List<double>();
            foreach (League league in dbContext.Leagues)
            {
                residuals.AddRange(league.GetAwayResiduals(date));
            }

            return residuals;
        }

        public void SetHistoryFiles()
        {
            List<string> yearCodes = new List<string> { "1415", "1516", "1617", "1718", "1819", "1920" };
            List<string> leagueCodes = new List<string>
            {
                "E0", "E1", "E2", "E3", "EC", "SC0", "SC1", "SC2", "SC3",
                "D1", "D2", "I1", "I2", "SP1", "SP2", "F1", "F2", "N1", "B1", "P1", "T1", "G1"
            };

            foreach(string leagueCode in leagueCodes)
            {
                historyFiles.Add(leagueCode, new List<string>());
                foreach (string yearCode in yearCodes)
                {
                    historyFiles[leagueCode].Add(string.Format("http://www.football-data.co.uk/mmz4281/{0}/{1}.csv", yearCode, leagueCode));
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
                historyFiles.Add(leagueCode.Value, new List<string> { string.Format("http://www.football-data.co.uk/new/{0}.csv", leagueCode.Key) });
            }
        }

        public void SetFixturesFiles()
        {
            fixturesFiles.Add("http://www.football-data.co.uk/fixtures.csv");
            fixturesFiles.Add("http://www.football-data.co.uk/new_league_fixtures.csv");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                dbContext.Dispose();
            }

            disposed = true;
        }
    }
}
