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

        public void LoadHistoricalFile(String aFile)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                // TO DO: Add handling for exceptions
                    string htmlCode = client.DownloadString(aFile);
                    ParseHistoricalData(htmlCode);
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    MessageBox.Show("Could not download file: " + aFile);
                }
                else
                {
                    string statusCode = ((HttpWebResponse)(ex.Response)).StatusCode.ToString();
                    MessageBox.Show("Could not download file: " + aFile + ". Error:" + statusCode);
                }
            }
        }

        public void LoadUpcomingFixturesFile()
        {
            using (WebClient client = new WebClient())         // download upcoming fixture list
            {
                fixtureList = new List<Fixture>();
                foreach (String fixturesFile in fixturesFiles)
                {
                    String htmlCode = client.DownloadString(fixturesFile);
                    ParseUpcomingFixtures(htmlCode);
                }
            }
        }

        public void AddFixture(Fixture fixture)
        {
            if (leagues.Count(x => x.LeagueID == fixture.LeagueID) == 0)
            {
                // if no match found then add league with the fixture's league ID
                AddLeague(new League(fixture.LeagueID));
            }

            foreach (League league in leagues)
            {
                if (league.LeagueID == fixture.LeagueID)
                {
                    league.AddFixture(fixture);
                }
            }
        }

        public void AddLeague(League league)
        {
            if(leagues.Find(x => x.LeagueID == league.LeagueID) == null)
            {
                leagues.Add(league);
            }
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

        public void ParseHistoricalData(String htmlCode)
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
                    League aLeague = GetLeague(league_code);
                    if (aLeague != null)
                    {
                        aLeague.ParseHistoricalData(fixture_data);
                    }
                    else
                    {
                        League newLeague = new League(league_code);
                        newLeague.ParseHistoricalData(fixture_data);
                        AddLeague(newLeague);
                    }
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
                errors.Add(fixture.average_home_residual);
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
                errors.Add(fixture.average_away_residual);
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

        /* Reads in fixture data for upcoming fixtures. Obviously these will not have goals scored or conceded and will just be dates, team names and odds.
         * Teams from upcoming fixtures are not actually added to the database - if they do not already exist in the database the fixture will be ignored */
        public void ParseUpcomingFixtures(String htmlCode)
        {
            League league = null;
            Team home_team = null;
            Team away_team = null;

            List<Bookmaker> odds = new List<Bookmaker>();
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

                var newLeague = fixture_data.Length == 15;
                var dateIndex = newLeague ? 2 : 1;
                var date_params = fixture_data[dateIndex].Split('/');

                DateTime date;
                if (newLeague)
                {
                    DateTime kickOffTime = Convert.ToDateTime(fixture_data[3]);
                    date = new DateTime(2000 + int.Parse(date_params[2]), int.Parse(date_params[1]), int.Parse(date_params[0]));
                    date = date.AddHours(kickOffTime.Hour);
                    date = date.AddMinutes(kickOffTime.Minute);
                }
                else
                {
                    date = new DateTime(2000 + int.Parse(date_params[2]), int.Parse(date_params[1]), int.Parse(date_params[0]));
                }
                
                String home_team_name = newLeague ? fixture_data[4] : fixture_data[2];
                String away_team_name = newLeague ? fixture_data[5] : fixture_data[3];

                for (int idx = 0; idx < fixture_data.Length; idx++)
                {
                    if (string.IsNullOrEmpty(fixture_data[idx]))
                    {
                        fixture_data[idx] = "0";
                    }
                }

                try
                {
                    List<Bookmaker> bookmakers = new List<Bookmaker>();
                    if (newLeague)
                    {
                        bookmakers.Add(new Bookmaker("Pinnacle Sport", double.Parse(fixture_data[6]), double.Parse(fixture_data[7]),
                                double.Parse(fixture_data[8])));
                        bookmakers.Add(new Bookmaker("OddsPortal", double.Parse(fixture_data[9]), double.Parse(fixture_data[10]),
                                double.Parse(fixture_data[11])));
                    }
                    else
                    {
                        bookmakers.Add(new Bookmaker("Bet 365", double.Parse(fixture_data[10]), double.Parse(fixture_data[11]),
                                double.Parse(fixture_data[12])));
                        bookmakers.Add(new Bookmaker("BetWin", double.Parse(fixture_data[13]), double.Parse(fixture_data[14]),
                            double.Parse(fixture_data[15])));
                        bookmakers.Add(new Bookmaker("InterWetten", double.Parse(fixture_data[16]), double.Parse(fixture_data[17]),
                            double.Parse(fixture_data[18])));
                        bookmakers.Add(new Bookmaker("Ladbrokes", double.Parse(fixture_data[19]), double.Parse(fixture_data[20]),
                            double.Parse(fixture_data[21])));
                        bookmakers.Add(new Bookmaker("Pinnacle Sport", double.Parse(fixture_data[22]), double.Parse(fixture_data[23]),
                            double.Parse(fixture_data[24])));
                        bookmakers.Add(new Bookmaker("William Hill", double.Parse(fixture_data[25]), double.Parse(fixture_data[26]),
                            double.Parse(fixture_data[27])));
                        bookmakers.Add(new Bookmaker("Stan James", double.Parse(fixture_data[28]), double.Parse(fixture_data[29]),
                            double.Parse(fixture_data[30])));
                    }
                    

                    foreach(Bookmaker bookmaker in bookmakers)
                    {
                        int index = DatabaseSettings.BookmakersUsed.IndexOf(bookmaker.Name);
                        if(index != -1)
                        {
                            odds.Add(bookmaker);
                        }
                    }
                }
                catch (FormatException ex)
                {
                    String a = ex.Message;
                }

                league = GetLeague(league_code);
                home_team = GetTeam(league_code, home_team_name);
                away_team = GetTeam(league_code, away_team_name);

                if (!((home_team == null) || (away_team == null)))
                {
                    fixtureList.Add(new Fixture(league, date, home_team, away_team, new Referee(""), odds));
                }
                
                }
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
