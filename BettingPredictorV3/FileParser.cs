using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace BettingPredictorV3
{
    public class FileParser
    {
        public FileParser()
        {
        }

        public delegate void ProgressUpdater(int fileNumber, int totalFiles, string fileName);

        public List<HistoricalFixtureDTO> ParseFiles(ProgressUpdater progressUpdater, IEnumerable<KeyValuePair<string, List<string>>> relevantFiles)
        {
            int fileNumber = 0;
            double progressAmount = 0.0;
            List<HistoricalFixtureDTO> fixtures = new List<HistoricalFixtureDTO>();

            // Only count the leagues that have upcoming fixtures
            int totalNumberOfFiles = relevantFiles.Sum(l => l.Value.Distinct().Count());

            foreach (var keyPair in relevantFiles)          // download and parse previous results
            {
                try
                {
                    var leagueFiles = keyPair.Value;
                    foreach (var file in leagueFiles)
                    {
                        fixtures.AddRange(LoadHistoricalFile(file));
                        progressUpdater(++fileNumber, totalNumberOfFiles, file);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return fixtures;
        }

        public List<HistoricalFixtureDTO> LoadHistoricalFile(string aFile)
        {
            List<HistoricalFixtureDTO> fixtures = new List<HistoricalFixtureDTO>();

            try
            {
                using (WebClient client = new WebClient())
                {
                    // TO DO: Add handling for exceptions
                    string htmlCode = client.DownloadString(aFile);
                    fixtures.AddRange(ParseHistoricalData(htmlCode));
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

            return fixtures;
        }

        public List<FixtureDTO> LoadUpcomingFixturesFile(List<string> fixturesFiles)
        {
            List<FixtureDTO> fixtures = new List<FixtureDTO>();

            using (WebClient client = new WebClient())         // download upcoming fixture list
            {
                foreach (string fixturesFile in fixturesFiles)
                {
                    string htmlCode = client.DownloadString(fixturesFile);
                    fixtures.AddRange(ParseUpcomingFixtures(htmlCode));
                }
            }

            return fixtures;
        }

        public List<HistoricalFixtureDTO> ParseHistoricalData(string htmlCode)
        {
            List<HistoricalFixtureDTO> fixtures = new List<HistoricalFixtureDTO>();
            int headings = htmlCode.IndexOf("\n");
            var columnHeaders = htmlCode.Substring(0, headings).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            htmlCode = htmlCode.Remove(0, headings + "\n".Length); // remove all column headings from the CSV file
            var csvRow = htmlCode.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string fixture in csvRow)
            {
                var fixtureData = fixture.Split(new[] { ',' }, StringSplitOptions.None);
                string leagueCode = fixtureData[0];
                if (leagueCode.Length > 0)
                {
                    Console.WriteLine(leagueCode + ": " + fixtureData.Length);
                    fixtures.Add(ParseHistoricalData(fixtureData, columnHeaders));
                }
            }

            return fixtures;
        }

        public static HistoricalFixtureDTO ParseHistoricalData(string[] fixtureData, List<string> columnHeaders)
        {
            Debug.Assert(fixtureData.Length == columnHeaders.Count);

            List<Bookmaker> odds = new List<Bookmaker>();

            /* Additional league data has been added at a later point, 
            these new leagues have less info in a different format */

            Console.WriteLine("[{0}]", string.Join("\", \"", fixtureData));

            var newLeague = fixtureData.Length == 19;
            var dateIndex = newLeague ? 3 : 1;
            var date_params = fixtureData[dateIndex].Split('/');
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

            string homeTeamName = newLeague ? fixtureData[5] : fixtureData[2 + extraColumnOffset];
            string awayTeamName = newLeague ? fixtureData[6] : fixtureData[3 + extraColumnOffset];

            for (int idx = 0; idx < fixtureData.Length; idx++)
            {
                if (string.IsNullOrEmpty(fixtureData[idx]))
                {
                    fixtureData[idx] = "0";
                }
            }

            try
            {
                List<Bookmaker> bookmakers = ParseBookmakers(fixtureData, columnHeaders);
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

            int homeGoals;
            int awayGoals;

            if (newLeague)
            {
                homeGoals = int.Parse(fixtureData[7]);
                awayGoals = int.Parse(fixtureData[8]);
            }
            else
            {
                homeGoals = int.Parse(fixtureData[4 + extraColumnOffset]);
                awayGoals = int.Parse(fixtureData[5 + extraColumnOffset]);

            }

            string leagueCode = fixtureData[0];
            return new HistoricalFixtureDTO
            {
                LeagueCode = leagueCode,
                Date = date,
                HomeTeamName = homeTeamName,
                AwayTeamName = awayTeamName,
                HomeGoals = homeGoals,
                AwayGoals = awayGoals,
                Odds = odds
            };
        }

        private static List<Bookmaker> ParseBookmakers(string[] fixtureData, List<string> columnHeaders)
        {
            List<Bookmaker> bookmakers = new List<Bookmaker>();

            if (columnHeaders.Contains("B365H"))
            {
                int oddsOffset = columnHeaders.IndexOf("B365H");
                bookmakers.Add(new Bookmaker("Bet 365", double.Parse(fixtureData[oddsOffset]), double.Parse(fixtureData[oddsOffset + 1]),
                                        double.Parse(fixtureData[oddsOffset + 2])));
            }

            if (columnHeaders.Contains("BWH"))
            {
                int oddsOffset = columnHeaders.IndexOf("BWH");
                bookmakers.Add(new Bookmaker("BetWin", double.Parse(fixtureData[oddsOffset]), double.Parse(fixtureData[oddsOffset + 1]),
                double.Parse(fixtureData[oddsOffset + 2])));
            }

            if (columnHeaders.Contains("IWH"))
            {
                int oddsOffset = columnHeaders.IndexOf("IWH");
                bookmakers.Add(new Bookmaker("InterWetten", double.Parse(fixtureData[oddsOffset]), double.Parse(fixtureData[oddsOffset + 1]),
                double.Parse(fixtureData[oddsOffset + 2])));
            }

            if (columnHeaders.Contains("LBH"))
            {
                int oddsOffset = columnHeaders.IndexOf("LBH");
                bookmakers.Add(new Bookmaker("Ladbrokes", double.Parse(fixtureData[oddsOffset]), double.Parse(fixtureData[oddsOffset + 1]),
                double.Parse(fixtureData[oddsOffset + 2])));
            }

            // The new leagues use PH for Pinnacle Sport, the old leagues use PSH!
            if (columnHeaders.Contains("PSH"))
            {
                int oddsOffset = columnHeaders.IndexOf("PSH");
                bookmakers.Add(new Bookmaker("Pinnacle Sport", double.Parse(fixtureData[oddsOffset]), double.Parse(fixtureData[oddsOffset + 1]),
                double.Parse(fixtureData[oddsOffset + 2])));
            }

            if (columnHeaders.Contains("PH"))
            {
                int oddsOffset = columnHeaders.IndexOf("PH");
                bookmakers.Add(new Bookmaker("Pinnacle Sport", double.Parse(fixtureData[oddsOffset]), double.Parse(fixtureData[oddsOffset + 1]),
                double.Parse(fixtureData[oddsOffset + 2])));
            }

            if (columnHeaders.Contains("WHH"))
            {
                int oddsOffset = columnHeaders.IndexOf("WHH");
                bookmakers.Add(new Bookmaker("William Hill", double.Parse(fixtureData[oddsOffset]), double.Parse(fixtureData[oddsOffset + 1]),
                double.Parse(fixtureData[oddsOffset + 2])));
            }

            if (columnHeaders.Contains("SJH"))
            {
                int oddsOffset = columnHeaders.IndexOf("SJH");
                bookmakers.Add(new Bookmaker("Stan James", double.Parse(fixtureData[oddsOffset]), double.Parse(fixtureData[oddsOffset + 1]),
                double.Parse(fixtureData[oddsOffset + 2])));
            }

            return bookmakers;
        }

        /* Reads in fixture data for upcoming fixtures. Obviously these will not have goals scored or conceded and will just be dates, team names and odds.
         * Teams from upcoming fixtures are not actually added to the database - if they do not already exist in the database the fixture will be ignored */
        public List<FixtureDTO> ParseUpcomingFixtures(string htmlCode)
        {
            int headings = htmlCode.IndexOf("\n");
            htmlCode = htmlCode.Remove(0, headings + "\n".Length); // remove all column headings from the CSV file
            var fixtureData = htmlCode.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return ParseUpcomingFixtures(fixtureData);
        }

        private List<FixtureDTO> ParseUpcomingFixtures(string[] fixturesData)
        {
            List<Bookmaker> odds = new List<Bookmaker>();
            List<FixtureDTO> fixtures = new List<FixtureDTO>();

            foreach (string fixture in fixturesData)
            {
                odds.Clear();

                var fixtureData = fixture.Split(new[] { ',' }, StringSplitOptions.None);
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

                fixtures.Add(new UpcomingFixtureDTO()
                {
                    LeagueCode = leagueCode,
                    Date = date,
                    HomeTeamName = homeTeamName,
                    AwayTeamName = awayTeamName,
                    Odds = odds
                });
            }

            return fixtures;
        }
    }
}
