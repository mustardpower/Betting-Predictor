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

            LoadUpcomingFixturesFile();

            // Only count the leagues that have upcoming fixtures
            var relevantFiles = database.HistoryFiles.Where(x => (database.LeagueCodes.Find(y => y == x.Key) != null));
            int totalNumberOfFiles = relevantFiles.Sum(l => l.Value.Distinct().Count());

            foreach (String leagueCode in database.LeagueCodes)          // download and parse previous results
            {
                try
                {
                    var leagueFiles = database.HistoryFiles[leagueCode];
                    foreach (var file in leagueFiles)
                    {
                        LoadHistoricalFile(file);
                        fileNumber++;
                        progressAmount = (double)fileNumber / (double)totalNumberOfFiles;
                        splash.SetProgress(progressAmount);
                        splash.SetText(System.String.Format("Loading historical data file number: {0} / {1} File Name: {2}", fileNumber, totalNumberOfFiles, file));
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
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
                var fixtureData = fixture.Split(new[] { ',' }, System.StringSplitOptions.None);
                String league_code = fixtureData[0];
                if (league_code.Length > 0)
                {
                    Console.WriteLine(league_code + ": " + fixtureData.Length);
                    database.AddLeague(league_code, fixtureData);
                }
            }
        }

        /* Reads in fixture data for upcoming fixtures. Obviously these will not have goals scored or conceded and will just be dates, team names and odds.
         * Teams from upcoming fixtures are not actually added to the database - if they do not already exist in the database the fixture will be ignored */
        public void ParseUpcomingFixtures(String htmlCode)
        {
            List<Bookmaker> odds = new List<Bookmaker>();
            int headings = htmlCode.IndexOf("\n");
            htmlCode = htmlCode.Remove(0, headings + "\n".Length); // remove all column headings from the CSV file
            var fixtures = htmlCode.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (String fixture in fixtures)
            {
                odds.Clear();

                var fixtureData = fixture.Split(new[] { ',' }, System.StringSplitOptions.None);
                String leagueCode = fixtureData[0];
                if (leagueCode.Length == 0)
                {
                    continue;
                }

                var newLeague = fixtureData.Length == 15;
                var dateIndex = newLeague ? 2 : 1;
                var dateParams = fixtureData[dateIndex].Split('/');

                DateTime date;
                if (newLeague)
                {
                    DateTime kickOffTime = Convert.ToDateTime(fixtureData[3]);
                    date = new DateTime(int.Parse(dateParams[2]), int.Parse(dateParams[1]), int.Parse(dateParams[0]));
                    date = date.AddHours(kickOffTime.Hour);
                    date = date.AddMinutes(kickOffTime.Minute);
                }
                else
                {
                    date = new DateTime(int.Parse(dateParams[2]), int.Parse(dateParams[1]), int.Parse(dateParams[0]));
                }

                if(DatabaseSettings.IgnorePlayedFixtures)
                {
                    if(date < DateTime.Today)
                    {
                        continue;
                    }
                }

                String homeTeamName = newLeague ? fixtureData[4] : fixtureData[2];
                String awayTeamName = newLeague ? fixtureData[5] : fixtureData[3];

                for (int idx = 0; idx < fixtureData.Length; idx++)
                {
                    if (string.IsNullOrEmpty(fixtureData[idx]))
                    {
                        fixtureData[idx] = "0";
                    }
                }

                try
                {
                    List<Bookmaker> bookmakers = new List<Bookmaker>();
                    if (newLeague)
                    {
                        bookmakers.Add(new Bookmaker("Pinnacle Sport", double.Parse(fixtureData[6]), double.Parse(fixtureData[7]),
                                double.Parse(fixtureData[8])));
                        bookmakers.Add(new Bookmaker("OddsPortal", double.Parse(fixtureData[9]), double.Parse(fixtureData[10]),
                                double.Parse(fixtureData[11])));
                    }
                    else
                    {
                        bookmakers.Add(new Bookmaker("Bet 365", double.Parse(fixtureData[10]), double.Parse(fixtureData[11]),
                                double.Parse(fixtureData[12])));
                        bookmakers.Add(new Bookmaker("BetWin", double.Parse(fixtureData[13]), double.Parse(fixtureData[14]),
                            double.Parse(fixtureData[15])));
                        bookmakers.Add(new Bookmaker("InterWetten", double.Parse(fixtureData[16]), double.Parse(fixtureData[17]),
                            double.Parse(fixtureData[18])));
                        /*bookmakers.Add(new Bookmaker("Ladbrokes", double.Parse(fixtureData[19]), double.Parse(fixtureData[20]),
                            double.Parse(fixtureData[21])));*/
                        bookmakers.Add(new Bookmaker("Pinnacle Sport", double.Parse(fixtureData[19]), double.Parse(fixtureData[20]),
                            double.Parse(fixtureData[21])));
                        bookmakers.Add(new Bookmaker("William Hill", double.Parse(fixtureData[22]), double.Parse(fixtureData[23]),
                            double.Parse(fixtureData[24])));
                        bookmakers.Add(new Bookmaker("Victor Chandler", double.Parse(fixtureData[25]), double.Parse(fixtureData[26]),
                            double.Parse(fixtureData[27])));
                        /*bookmakers.Add(new Bookmaker("Stan James", double.Parse(fixtureData[28]), double.Parse(fixtureData[29]),
                            double.Parse(fixtureData[30])));*/
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

                database.AddUpcomingFixture(leagueCode, date, homeTeamName, awayTeamName, odds);
            }
        }
    }
}
