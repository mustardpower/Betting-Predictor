using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace BettingPredictorV3
{
    class FileParser : IFileParser
    {
        Database database;

        public Database PopulateDatabase(Database database, Splash splash)
        {
            this.database = database;

            ParseFiles(database, splash);

            return database;
        }

        public void ParseFiles(Database database, Splash splash)
        {
            int fileNumber = 0;
            double progressAmount = 0.0;

            foreach (String file in database.HistoryFiles)          // download and parse previous results
            {
                LoadHistoricalFile(file);
                fileNumber++;
                progressAmount = (double)fileNumber / (double)database.HistoryFiles.Count;
                splash.SetProgress(progressAmount);
                splash.SetText(System.String.Format("Loading historical data file number: {0} / {1} File Name: {2}", fileNumber, database.HistoryFiles.Count, file));
            }

            LoadUpcomingFixturesFile();
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
                List<Fixture> fixtureList = new List<Fixture>();
                foreach (String fixturesFile in database.FixtureFiles)
                {
                    String htmlCode = client.DownloadString(fixturesFile);
                    ParseUpcomingFixtures(htmlCode);
                }
            }
        }

        public void ParseHistoricalData(String htmlCode)
        {
            int headings = htmlCode.IndexOf("\n");
            htmlCode = htmlCode.Remove(0, headings + "\n".Length); // remove all column headings from the CSV file
            var fixtures = htmlCode.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (String fixture in fixtures)
            {
                var fixture_data = fixture.Split(new[] { ',' }, System.StringSplitOptions.None);
                String league_code = fixture_data[0];
                if (league_code.Length > 0)
                {
                    Console.WriteLine(league_code + ": " + fixture_data.Length);
                    League aLeague = database.GetLeague(league_code);
                    if (aLeague != null)
                    {
                        aLeague.ParseHistoricalData(fixture_data);
                    }
                    else
                    {
                        League newLeague = new League(league_code);
                        newLeague.ParseHistoricalData(fixture_data);
                        database.AddLeague(newLeague);
                    }
                }
            }
        }

        /* Reads in fixture data for upcoming fixtures. Obviously these will not have goals scored or conceded and will just be dates, team names and odds.
         * Teams from upcoming fixtures are not actually added to the database - if they do not already exist in the database the fixture will be ignored */
        public void ParseUpcomingFixtures(String htmlCode)
        {
            League league = null;
            Team homeTeam = null;
            Team awayTeam = null;

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


                    foreach (Bookmaker bookmaker in bookmakers)
                    {
                        int index = DatabaseSettings.BookmakersUsed.IndexOf(bookmaker.Name);
                        if (index != -1)
                        {
                            odds.Add(bookmaker);
                        }
                    }
                }
                catch (FormatException ex)
                {
                    String a = ex.Message;
                }

                league = database.GetLeague(league_code);
                homeTeam = database.GetTeam(league_code, home_team_name);
                awayTeam = database.GetTeam(league_code, away_team_name);

                if (!((homeTeam == null) || (awayTeam == null)))
                {
                    database.AddUpcomingFixture(league, date, homeTeam, awayTeam, odds);
                }

            }
        }
    }
}
