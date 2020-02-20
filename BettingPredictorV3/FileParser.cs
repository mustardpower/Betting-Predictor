using BettingPredictorV3.DataStructures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace BettingPredictorV3
{
    public class FileParser : IFileParser
    {
        public Database Database { get; set; }

        public FileParser()
        {
            Database = new Database();
        }

        public Database PopulateDatabase(Database database, Splash splash)
        {
            Database = database;

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
                    MessageBox.Show("Could not download file: " + aFile + ". Error: " + statusCode);
                }
            }
            catch(FormatException ex)
            {
                MessageBox.Show("Could not download file: " + aFile + ".\n\n Error: " + ex.Message);
            }
        }

        public void LoadUpcomingFixturesFile()
        {
            using (WebClient client = new WebClient())         // download upcoming fixture list
            {
                foreach (String fixturesFile in Database.FixtureFiles)
                {
                    String htmlCode = client.DownloadString(fixturesFile);
                    ParseUpcomingFixtures(htmlCode);
                }
            }
        }

        public void ParseHistoricalData(string htmlCode)
        {
            int headings = htmlCode.IndexOf("\n");
            htmlCode = htmlCode.Remove(0, headings + "\n".Length); // remove all column headings from the CSV file
            var fixtures = htmlCode.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            Database.AddFixtures(fixtures);
            
        }

        // Creates a Fixture from the fixture data
        public Fixture ParseHistoricalFixtureData(League aLeague, string[] fixture_data)
        {
            List<Bookmaker> odds = new List<Bookmaker>();

            /* Additional league data has been added at a later point, 
            these new leagues have less info in a different format */

            Console.WriteLine("[{0}]", string.Join("\", \"", fixture_data));

            var newLeague = fixture_data.Length == 19;
            var dateIndex = newLeague ? 3 : 1;
            var date_params = fixture_data[dateIndex].Split('/');
            int yearParam = int.Parse(date_params[2]);
            const int kMinimumYearPossible = 1990;
            if (yearParam < kMinimumYearPossible)
            {
                yearParam += 2000;
            }
            DateTime date = new DateTime(yearParam, int.Parse(date_params[1]), int.Parse(date_params[0]));
            Debug.Assert(date.Year > 1990);
            Debug.Assert(date.Year < 3000);

            // kick off times were added for old leagues for the 2019/20 season
            DateTime dateThatKickOffTimeWasAdded = new DateTime(2019, 7, 1);
            int extraColumnOffset = date > dateThatKickOffTimeWasAdded ? 1 : 0;

            String home_team_name = newLeague ? fixture_data[5] : fixture_data[2 + extraColumnOffset];
            String away_team_name = newLeague ? fixture_data[6] : fixture_data[3 + extraColumnOffset];

            for (int idx = 0; idx < fixture_data.Length; idx++)
            {
                if (string.IsNullOrEmpty(fixture_data[idx]))
                {
                    fixture_data[idx] = "0";
                }
            }

            int offset = GetFileOffset(fixture_data);

            try
            {
                List<Bookmaker> bookmakers = ParseBookmakers(fixture_data, newLeague, offset);
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

            int home_goals;
            int away_goals;

            if (newLeague)
            {
                home_goals = int.Parse(fixture_data[7]);
                away_goals = int.Parse(fixture_data[8]);
            }
            else
            {
                home_goals = int.Parse(fixture_data[4 + extraColumnOffset]);
                away_goals = int.Parse(fixture_data[5 + extraColumnOffset]);

            }

            var newFixture = new Fixture(
                aLeague, 
                date,
                new Team(home_team_name),
                new Team(away_team_name),
                home_goals, 
                away_goals,
                new Referee(""),
                odds);

            return newFixture;
        }

        public int GetFileOffset(string[] fixture_data)
        {
            if (fixture_data.Length == 49 || fixture_data.Length == 52)
            {
                return 0;
            }
            else if (fixture_data.Length == 64)
            {
                return 12;
            }
            else if (fixture_data.Length == 62 || fixture_data.Length == 65)
            {
                return 13;
            }
            else if (fixture_data.Length == 19)
            {
                return 0;
            }
            else
            {
                return fixture_data.Length - 52;
            }
        }

        private static List<Bookmaker> ParseBookmakers(string[] fixture_data, bool newLeague, int offset)
        {
            List<Bookmaker> bookmakers;
            if (newLeague)
            {
                bookmakers = ParseBookmakersNewLeague(fixture_data, offset);
            }
            else
            {
                bookmakers = ParseBookmakersClassicLeague(fixture_data, offset);
            }
            return bookmakers;
        }

        private static List<Bookmaker> ParseBookmakersClassicLeague(string[] fixture_data, int offset)
        {
            List<Bookmaker> bookmakers = new List<Bookmaker>();
            bookmakers.Add(new Bookmaker("Bet 365", double.Parse(fixture_data[10 + offset]), double.Parse(fixture_data[11 + offset]),
                                        double.Parse(fixture_data[12 + offset])));
            bookmakers.Add(new Bookmaker("BetWin", double.Parse(fixture_data[13 + offset]), double.Parse(fixture_data[14 + offset]),
                double.Parse(fixture_data[15 + offset])));
            bookmakers.Add(new Bookmaker("InterWetten", double.Parse(fixture_data[16 + offset]), double.Parse(fixture_data[17 + offset]),
                double.Parse(fixture_data[18 + offset])));
            bookmakers.Add(new Bookmaker("Ladbrokes", double.Parse(fixture_data[19 + offset]), double.Parse(fixture_data[20 + offset]),
                double.Parse(fixture_data[21 + offset])));
            bookmakers.Add(new Bookmaker("Pinnacle Sport", double.Parse(fixture_data[22 + offset]), double.Parse(fixture_data[23 + offset]),
                double.Parse(fixture_data[24 + offset])));
            bookmakers.Add(new Bookmaker("William Hill", double.Parse(fixture_data[25 + offset]), double.Parse(fixture_data[26 + offset]),
                double.Parse(fixture_data[27 + offset])));
            bookmakers.Add(new Bookmaker("Stan James", double.Parse(fixture_data[28 + offset]), double.Parse(fixture_data[29 + offset]),
                double.Parse(fixture_data[30 + offset])));
            return bookmakers;
        }

        private static List<Bookmaker> ParseBookmakersNewLeague(string[] fixture_data, int offset)
        {
            List<Bookmaker> bookmakers = new List<Bookmaker>();
            bookmakers.Add(new Bookmaker("Best Odds", double.Parse(fixture_data[10 + offset]), double.Parse(fixture_data[11 + offset]),
                                        double.Parse(fixture_data[12 + offset])));
            return bookmakers;
        }

        /* Reads in fixture data for upcoming fixtures. Obviously these will not have goals scored or conceded and will just be dates, team names and odds.
         * Teams from upcoming fixtures are not actually added to the database - if they do not already exist in the database the fixture will be ignored */
        public void ParseUpcomingFixtures(string htmlCode)
        {
            int headings = htmlCode.IndexOf("\n");
            htmlCode = htmlCode.Remove(0, headings + "\n".Length); // remove all column headings from the CSV file
            var fixtures = htmlCode.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            ParseUpcomingFixtures(fixtures);
        }

        private void ParseUpcomingFixtures(string[] fixtures)
        {
            List<Bookmaker> odds = new List<Bookmaker>();
            foreach (string fixture in fixtures)
            {
                odds.Clear();

                var fixtureData = fixture.Split(new[] { ',' }, System.StringSplitOptions.None);
                string leagueCode = fixtureData[0];
                if (leagueCode.Length == 0)
                {
                    continue;
                }

                var newLeague = fixtureData.Length == 15;
                var dateIndex = newLeague ? 2 : 1;
                var dateParams = fixtureData[dateIndex].Split('/');

                DateTime date;
                DateTime kickOffTime;
                if (newLeague)
                {
                    kickOffTime = Convert.ToDateTime(fixtureData[3]);
                    date = new DateTime(int.Parse(dateParams[2]), int.Parse(dateParams[1]), int.Parse(dateParams[0]));
                }
                else
                {
                    kickOffTime = Convert.ToDateTime(fixtureData[2]);
                    date = new DateTime(int.Parse(dateParams[2]), int.Parse(dateParams[1]), int.Parse(dateParams[0]));
                }

                date = date.AddHours(kickOffTime.Hour);
                date = date.AddMinutes(kickOffTime.Minute);

                if (DatabaseSettings.IgnorePlayedFixtures)
                {
                    if (date < DateTime.Today)
                    {
                        continue;
                    }
                }

                string homeTeamName = newLeague ? fixtureData[4] : fixtureData[3];
                string awayTeamName = newLeague ? fixtureData[5] : fixtureData[4];

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
                        bookmakers.Add(new Bookmaker("Bet 365", double.Parse(fixtureData[11]), double.Parse(fixtureData[12]),
                                double.Parse(fixtureData[13])));
                        bookmakers.Add(new Bookmaker("BetWin", double.Parse(fixtureData[14]), double.Parse(fixtureData[15]),
                            double.Parse(fixtureData[16])));
                        bookmakers.Add(new Bookmaker("InterWetten", double.Parse(fixtureData[17]), double.Parse(fixtureData[18]),
                            double.Parse(fixtureData[19])));
                        /*bookmakers.Add(new Bookmaker("Ladbrokes", double.Parse(fixtureData[19]), double.Parse(fixtureData[20]),
                            double.Parse(fixtureData[21])));*/
                        bookmakers.Add(new Bookmaker("Pinnacle Sport", double.Parse(fixtureData[20]), double.Parse(fixtureData[21]),
                            double.Parse(fixtureData[22])));
                        bookmakers.Add(new Bookmaker("William Hill", double.Parse(fixtureData[23]), double.Parse(fixtureData[24]),
                            double.Parse(fixtureData[25])));
                        bookmakers.Add(new Bookmaker("Victor Chandler", double.Parse(fixtureData[26]), double.Parse(fixtureData[27]),
                            double.Parse(fixtureData[28])));
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
                    string a = ex.Message;
                }

                Database.AddUpcomingFixture(leagueCode, date, homeTeamName, awayTeamName, odds);
            }
        }
    }
}
