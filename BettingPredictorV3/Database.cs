using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;

namespace BettingPredictorV3
{
    public class Database
    {
        private List<League> leagues;   // list of all the leagues stored
        private List<Fixture> fixtureList;
        List<String> historyFiles;
        String fixturesFile;

        public Database()
        {
            leagues = new List<League>();
            fixtureList = new List<Fixture>();
            historyFiles = new List<String>();
        }

        public void clearData()
        {
            leagues.Clear();
        }

        public List<String> getHistoricalFiles()
        {
            return historyFiles;
        }

        public void loadHistoricalFile(String aFile)
        {
            using (WebClient client = new WebClient())
            {
                string htmlCode = client.DownloadString(aFile);
                parseHistoricalData(htmlCode);
            }
        }

        public void loadUpcomingFixturesFile()
        {
            using (WebClient client = new WebClient())         // download upcoming fixture list
            {
                String htmlCode = client.DownloadString(fixturesFile);
                parseUpcomingFixtures(htmlCode);
            }
        }

        public void addFixture(Fixture fixture)
        {
            if (leagues.Count(x => x.LeagueID == fixture.getLeagueID()) == 0)
            {
                // if no match found then add league with the fixture's league ID
                addLeague(new League(fixture.getLeagueID()));
            }

            foreach (League league in leagues)
            {
                if (league.LeagueID == fixture.getLeagueID())
                {
                    league.addFixture(fixture);
                }
            }
        }

        public void addLeague(League league)
        {
            if(leagues.Find(x => x.LeagueID == league.LeagueID) == null)
            {
                leagues.Add(league);
            }
        }

        public void addTeam(Team team)
        {
            foreach (League league in leagues)
            {
                if (league.LeagueID == team.LeagueID)
                {
                    league.addTeam(team);
                }
            }
        }

        public void parseHistoricalData(String htmlCode)
        {
            int headings = htmlCode.IndexOf("\n");
            htmlCode = htmlCode.Remove(0,headings+"\n".Length); // remove all column headings from the CSV file
            var fixtures = htmlCode.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (String fixture in fixtures)
            {
                var fixture_data = fixture.Split(new[]{','},System.StringSplitOptions.None);
                String league_code = fixture_data[0];
                if (league_code.Length > 0)
                {
                    Console.WriteLine(league_code + ": " + fixture_data.Length);
                    League aLeague = getLeague(league_code);
                    if (aLeague != null)
                    {
                        aLeague.parseHistoricalData(fixture_data);
                    }
                    else
                    {
                        League newLeague = new League(league_code);
                        newLeague.parseHistoricalData(fixture_data);
                        addLeague(newLeague);
                    }
                }
            }
        }

        public List<League> getLeagues()
        {
            return leagues;
        }
        public List<Fixture> getPreviousResults()
        {
            List<Fixture> fixtures = new List<Fixture>();
            foreach (League league in leagues)
            {
                fixtures.AddRange(league.getFixtures());
            }

            return fixtures;
        }

        public List<Fixture> getFixtures()
        {
            return fixtureList;
        }

        public Team getTeam(String league_code, String team_name)
        {
            League league = leagues.Find(x => x.LeagueID == league_code);
            return league.getTeam(team_name);
        }

        public League getLeague(String league_code)
        {
            return leagues.Find(x => x.LeagueID == league_code);
        }

        public void predictResults(double alpha,double beta)
        {
            // predict the results for historical fixtures - used to find profitable betting areas 
            predictHistoricalResults(alpha, beta);
            // predict the upcoming fixtures
            predictUpcomingFixtures(alpha, beta);
        }

        public void predictHistoricalResults(double alpha, double beta)
        {
            foreach (League league in leagues)
            {
                league.predictResults(alpha, beta);
            }
        }

        public void predictUpcomingFixtures(double alpha, double beta)
        {
            foreach (Fixture fixture in fixtureList)
            {
                fixture.predictResult(alpha, beta);
            }
        }

        public double getAlphaValue()
        {
            double alpha;
            List<double> errors = new List<double>();

            foreach (Fixture fixture in fixtureList)
            {
                errors.Add(fixture.average_home_residual);
            }

            errors.RemoveAll(x => Double.IsNaN(x));
            alpha = errors.Average();
                
            return alpha;
        }

        public double getBetaValue()
        {
            double beta;
            List<double> errors = new List<double>();

            foreach (Fixture fixture in fixtureList)
            {
                errors.Add(fixture.average_away_residual);
            }

            errors.RemoveAll(x => Double.IsNaN(x));
            beta = errors.Average();

            return beta;
        }

        public List<double> getHomeResiduals(DateTime date)
        {
            List<double> residuals = new List<double>();
            foreach (League league in leagues)
            {
                residuals.AddRange(league.getHomeResiduals(date));
            }

            return residuals;
        }
        public List<double> getAwayResiduals(DateTime date)
        {
            List<double> residuals = new List<double>();
            foreach (League league in leagues)
            {
                residuals.AddRange(league.getAwayResiduals(date));
            }

            return residuals;
        }

        /* Reads in fixture data for upcoming fixtures. Obviously these will not have goals scored or conceded and will just be dates, team names and odds.
         * Teams from upcoming fixtures are not actually added to the database - if they do not already exist in the database the fixture will be ignored */
        public void parseUpcomingFixtures(String htmlCode)
        {
            League league = null;
            Team home_team = null;
            Team away_team = null;

            List<Bookmaker> odds = new List<Bookmaker>();
            fixtureList = new List<Fixture>();
            int headings = htmlCode.IndexOf("\n");
            htmlCode = htmlCode.Remove(0, headings + "\n".Length); // remove all column headings from the CSV file
            var fixtures = htmlCode.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (String fixture in fixtures)
            {
                odds.Clear();
               
                var fixture_data = fixture.Split(new[] { ',' }, System.StringSplitOptions.None);
                String league_code = fixture_data[0];
                if (league_code.Length == 0)
                {
                    continue;
                }
                var date_params = fixture_data[1].Split('/');
                DateTime date = new DateTime(2000 + int.Parse(date_params[2]), int.Parse(date_params[1]), int.Parse(date_params[0]));
                String home_team_name = fixture_data[2];
                String away_team_name = fixture_data[3];

                for (int idx = 0; idx < fixture_data.Length; idx++)
                {
                    if (string.IsNullOrEmpty(fixture_data[idx]))
                    {
                        fixture_data[idx] = "0";
                    }
                }

                try
                {

                    Bookmaker bet_365 = new Bookmaker("Bet 365",double.Parse(fixture_data[10]), double.Parse(fixture_data[11]),
                            double.Parse(fixture_data[12]));
                    Bookmaker bet_win = new Bookmaker("BetWin",double.Parse(fixture_data[13]), double.Parse(fixture_data[14]),
                        double.Parse(fixture_data[15]));
                    Bookmaker inter_wetten = new Bookmaker("InterWetten",double.Parse(fixture_data[16]), double.Parse(fixture_data[17]),
                        double.Parse(fixture_data[18]));
                    Bookmaker ladbrokes = new Bookmaker("Ladbrokes",double.Parse(fixture_data[19]), double.Parse(fixture_data[20]),
                        double.Parse(fixture_data[21]));
                    Bookmaker pinnacle_sport = new Bookmaker("Pinnacle Sport",double.Parse(fixture_data[22]), double.Parse(fixture_data[23]),
                        double.Parse(fixture_data[24]));
                    Bookmaker william_hill = new Bookmaker("William Hill",double.Parse(fixture_data[25]), double.Parse(fixture_data[26]),
                        double.Parse(fixture_data[27]));
                    Bookmaker stan_james = new Bookmaker("Stan James",double.Parse(fixture_data[28]), double.Parse(fixture_data[29]),
                        double.Parse(fixture_data[30]));

                    odds.Add(bet_365);
                    odds.Add(bet_win);
                    odds.Add(inter_wetten);
                    odds.Add(ladbrokes);
                    odds.Add(pinnacle_sport);
                    odds.Add(william_hill);
                    odds.Add(stan_james);
                }
                catch (FormatException ex)
                {
                    String a = ex.Message;
                }

                league = getLeague(league_code);
                home_team = getTeam(league_code, home_team_name);
                away_team = getTeam(league_code, away_team_name);

                if (!((home_team == null) || (away_team == null)))
                {
                    fixtureList.Add(new Fixture(league, date, home_team, away_team, new Referee(""), odds));
                }
                
                }
            }

        public void setHistoryFiles()
        {     
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
        }

        public void setFixturesFile()
        {
            fixturesFile = "http://www.football-data.co.uk/fixtures.csv";
        }
    }
}
