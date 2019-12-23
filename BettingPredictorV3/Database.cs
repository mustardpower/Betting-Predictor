﻿using System;
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

        public List<League> Leagues { get; set; }

        public List<string> FixtureFiles { get; set; }

        public Dictionary<string, List<string>> HistoryFiles { get; set; }

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
            HistoryFiles = new Dictionary<String, List<String>>();
            FixtureFiles = new List<String>();
        }

        public void ClearData()
        {
            Leagues.Clear();
        }

        public void AddUpcomingFixture(string leagueCode, DateTime date, string homeTeamName, string awayTeamName, List<Bookmaker> odds)
        {
            League league = GetLeague(leagueCode);
            if(league == null)
            {
                League newLeague = new League(leagueCode);
                Leagues.Add(newLeague);
                league = newLeague;
            }

            Team homeTeam = GetTeam(leagueCode, homeTeamName);
            Team awayTeam = GetTeam(leagueCode, awayTeamName);

            if (homeTeam == null)
            {
                league.AddTeam(new Team(league, homeTeamName));
                homeTeam = GetTeam(leagueCode, homeTeamName);
            }
            if(awayTeam == null)
            {
                league.AddTeam(new Team(league, awayTeamName));
                awayTeam = GetTeam(leagueCode, awayTeamName);
            }

            FixtureList.Add(new Fixture(league, date, homeTeam, awayTeam, new Referee(""), odds));
        }

        public void AddLeague(string leagueCode, List<string> columnHeaders, string[] fixtureData)
        {
            League aLeague = GetLeague(leagueCode);
            if (aLeague != null)
            {
                aLeague.ParseHistoricalData(fixtureData, columnHeaders);
            }
            else
            {
                League newLeague = new League(leagueCode);
                newLeague.ParseHistoricalData(fixtureData, columnHeaders);
                Leagues.Add(newLeague);
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
            return new ProfitLossInterval(intervalName, "Home", fixtures.Count, profit, yield);
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

        public Team GetTeam(String leagueCode, String teamName)
        {
            League league = Leagues.Find(x => x.LeagueID == leagueCode);
            if(league == null){ return null; }
            return league.GetTeam(teamName);
        }

        public League GetLeague(String leagueCode)
        {
            return Leagues.Find(x => x.LeagueID == leagueCode);
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
            foreach (League league in Leagues)
            {
                league.PredictResults(alpha, beta);
            }
        }

        public void PredictUpcomingFixtures(double alpha, double beta)
        {
            ResultPredictor resultPredictor = new ResultPredictor();
            foreach (Fixture fixture in FixtureList)
            {
                resultPredictor.PredictResult(fixture, alpha, beta);
            }
        }

        public double GetAlphaValue()
        {
            double alpha = 0.0;
            List<double> errors = new List<double>();

            foreach (Fixture fixture in FixtureList)
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

            foreach (Fixture fixture in FixtureList)
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
            foreach (League league in Leagues)
            {
                residuals.AddRange(league.GetHomeResiduals(date));
            }

            return residuals;
        }

        public List<double> GetAwayResiduals(DateTime date)
        {
            List<double> residuals = new List<double>();
            foreach (League league in Leagues)
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
                HistoryFiles.Add(leagueCode, new List<string>());
                foreach (string yearCode in yearCodes)
                {
                    HistoryFiles[leagueCode].Add(string.Format("http://www.football-data.co.uk/mmz4281/{0}/{1}.csv", yearCode, leagueCode));
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
                HistoryFiles.Add(leagueCode.Value, new List<string> { string.Format("http://www.football-data.co.uk/new/{0}.csv", leagueCode.Key) });
            }
        }

        public void SetFixturesFiles()
        {
            FixtureFiles.Add("http://www.football-data.co.uk/fixtures.csv");
            FixtureFiles.Add("http://www.football-data.co.uk/new_league_fixtures.csv");
        }
    }
}
