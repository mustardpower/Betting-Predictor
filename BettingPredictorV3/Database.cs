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
    public class Database
    {
        private List<League> leagues;   // list of all the leagues stored
        private List<Fixture> fixtureList;
        private List<String> fixturesFiles;
        private List<String> historyFiles;

        public List<League> Leagues
        {
            get
            {
                return leagues;
            }
        }

        public List<String> FixtureFiles
        {
            get
            {
                return fixturesFiles;
            }
        }

        public List<String> HistoryFiles
        {
            get
            {
                return historyFiles;
            }
        }

        public List<Fixture> FixtureList
        {
            get
            {
                return fixtureList;
            }
        }

        public Database()
        {
            leagues = new List<League>();
            fixtureList = new List<Fixture>();
            historyFiles = new List<String>();
            fixturesFiles = new List<String>();
            DatabaseSettings.BookmakersUsed = DatabaseSettings.DefaultBookmakers();
        }

        public void ClearData()
        {
            leagues.Clear();
        }

        public void AddUpcomingFixture(string leagueCode, DateTime date, string homeTeamName, string awayTeamName, List<Bookmaker> odds)
        {
            League league = GetLeague(leagueCode);
            Team homeTeam = GetTeam(leagueCode, homeTeamName);
            Team awayTeam = GetTeam(leagueCode, awayTeamName);

            if (!((homeTeam == null) || (awayTeam == null)))
            {
                fixtureList.Add(new Fixture(league, date, homeTeam, awayTeam, new Referee(""), odds));
            }
        }

        public void AddLeague(string leagueCode, string[] fixtureData)
        {
            League aLeague = GetLeague(leagueCode);
            if (aLeague != null)
            {
                aLeague.ParseHistoricalData(fixtureData);
            }
            else
            {
                League newLeague = new League(leagueCode);
                newLeague.ParseHistoricalData(fixtureData);
                leagues.Add(newLeague);
            }
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
                bestHomeOdds = new Bookmaker("bookie", 0.0, 0.0, 0.0)
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

            double yield = profit / fixtures.Count;
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

            double yield = profit / fixtures.Count;
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
                x1 = x2;
                x2 += h;

                homeInterval.setRange(x1, x2);
                awayInterval.setRange(x1, x2);
                profitLossIntervals.Add(homeInterval);
                profitLossIntervals.Add(awayInterval);
            }

            return profitLossIntervals;
        }

        public void AddTeam(Team team)
        {
            foreach (League league in leagues)
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
            foreach (League league in leagues)
            {
                fixtures.AddRange(league.GetFixtures());
            }

            return fixtures;
        }

        public Team GetTeam(String leagueCode, String teamName)
        {
            League league = leagues.Find(x => x.LeagueID == leagueCode);
            if(league == null){ return null; }
            return league.GetTeam(teamName);
        }

        public League GetLeague(String leagueCode)
        {
            return leagues.Find(x => x.LeagueID == leagueCode);
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
            foreach (League league in leagues)
            {
                league.PredictResults(alpha, beta);
            }
        }

        public void PredictUpcomingFixtures(double alpha, double beta)
        {
            foreach (Fixture fixture in fixtureList)
            {
                fixture.PredictResult(alpha, beta);
            }
        }

        public double GetAlphaValue()
        {
            double alpha = 0.0;
            List<double> errors = new List<double>();

            foreach (Fixture fixture in fixtureList)
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

            foreach (Fixture fixture in fixtureList)
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
            foreach (League league in leagues)
            {
                residuals.AddRange(league.GetHomeResiduals(date));
            }

            return residuals;
        }

        public List<double> GetAwayResiduals(DateTime date)
        {
            List<double> residuals = new List<double>();
            foreach (League league in leagues)
            {
                residuals.AddRange(league.GetAwayResiduals(date));
            }

            return residuals;
        }

        public void SetHistoryFiles()
        {
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/E0.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/E1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/E2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/E3.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/EC.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/SC0.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/SC1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/SC2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/SC3.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/D1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/D2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/I1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/I2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/SP1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/SP2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/F1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/F2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/N1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/B1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/P1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/T1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1314/G1.csv");

            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/E0.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/E1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/E2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/E3.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/EC.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/SC0.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/SC1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/SC2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/SC3.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/D1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/D2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/I1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/I2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/SP1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/SP2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/F1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/F2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/N1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/B1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/P1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/T1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1415/G1.csv");

            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/E0.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/E1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/E2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/E3.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/EC.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/SC0.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/SC1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/SC2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/SC3.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/D1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/D2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/I1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/I2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/SP1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/SP2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/F1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/F2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/N1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/B1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/P1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/T1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1516/G1.csv");

            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/E0.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/E1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/E2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/E3.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/EC.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/SC0.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/SC1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/SC2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/SC3.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/D1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/D2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/I1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/I2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/SP1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/SP2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/F1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/F2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/N1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/B1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/P1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/T1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1617/G1.csv");

            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/E0.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/E1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/E2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/E3.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/EC.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/SC0.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/SC1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/SC2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/SC3.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/D1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/D2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/I1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/I2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/SP1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/SP2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/F1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/F2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/N1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/B1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/P1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/T1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1718/G1.csv");

            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/E0.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/E1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/E2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/E3.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/EC.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/SC0.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/SC1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/SC2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/SC3.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/D1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/D2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/I1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/I2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/SP1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/SP2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/F1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/F2.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/N1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/B1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/P1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/T1.csv");
            historyFiles.Add("http://www.football-data.co.uk/mmz4281/1819/G1.csv");

            historyFiles.Add("http://www.football-data.co.uk/new/ARG.csv");
            historyFiles.Add("http://www.football-data.co.uk/new/AUT.csv");
            historyFiles.Add("http://www.football-data.co.uk/new/BRA.csv");
            historyFiles.Add("http://www.football-data.co.uk/new/CHN.csv");
            historyFiles.Add("http://www.football-data.co.uk/new/DNK.csv");
            historyFiles.Add("http://www.football-data.co.uk/new/FIN.csv");
            historyFiles.Add("http://www.football-data.co.uk/new/IRL.csv");
            historyFiles.Add("http://www.football-data.co.uk/new/JPN.csv");
            historyFiles.Add("http://www.football-data.co.uk/new/MEX.csv");
            historyFiles.Add("http://www.football-data.co.uk/new/NOR.csv");
            historyFiles.Add("http://www.football-data.co.uk/new/POL.csv");
            historyFiles.Add("http://www.football-data.co.uk/new/ROU.csv");
            historyFiles.Add("http://www.football-data.co.uk/new/RUS.csv");
            historyFiles.Add("http://www.football-data.co.uk/new/SWE.csv");
            historyFiles.Add("http://www.football-data.co.uk/new/SWZ.csv");
            historyFiles.Add("http://www.football-data.co.uk/new/USA.csv");
        }

        public void SetFixturesFiles()
        {
            fixturesFiles.Add("http://www.football-data.co.uk/fixtures.csv");
            fixturesFiles.Add("http://www.football-data.co.uk/new_league_fixtures.csv");
        }
    }
}
