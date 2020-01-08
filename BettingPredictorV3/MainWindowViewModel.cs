using BettingPredictorV3.DataStructures;
using CsvFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BettingPredictorV3
{
    public class MainWindowViewModel
    {
        private Database database = new Database();

        public MainWindowViewModel(Database aDatabase)
        {
            database = aDatabase;
        }

        public IEnumerable<League> Leagues
        {
            get
            {
                return database.Leagues;
            }
        }

        public IEnumerable<Fixture> FixtureList
        {
            get
            {
                return database.FixtureList;
            }
        }

        public IEnumerable<DateTime> UpcomingFixtureDates
        {
            get
            {
                return DefaultUpcomingFixtures.Select(x => x.Date.DayOfYear).Distinct().Select(dayOfYear => new DateTime(DateTime.Now.Year, 1, 1).AddDays(dayOfYear - 1));
            }
        }


        public List<Fixture> DefaultUpcomingFixtures
        {
            get
            {
                List<Fixture> upcomingFixtures = new List<Fixture>();
                upcomingFixtures = database.FixtureList;
                // remove teams with less than a season of results
                upcomingFixtures.RemoveAll(x => x.HomeTeam.GetFixturesBefore(DateTime.Now).Count < 19);
                upcomingFixtures.RemoveAll(x => x.AwayTeam.GetFixturesBefore(DateTime.Now).Count < 19);
                return upcomingFixtures;
            }
        }

        public List<Fixture> PreviousFixtures
        {
            get
            {
                return GetPreviousFixturesWithMinimumFixtures(minimumNumberOfFixtures: 10);
            }
        }

        public List<Fixture> GetPreviousFixturesWithMinimumFixtures(int minimumNumberOfFixtures)
        {
            List<Fixture> previousFixtures = database.GetPreviousResults();
            previousFixtures.RemoveAll(x => x.HomeTeam.GetFixturesBefore(x.Date).Count < minimumNumberOfFixtures);
            previousFixtures.RemoveAll(x => x.AwayTeam.GetFixturesBefore(x.Date).Count < minimumNumberOfFixtures);
            previousFixtures = previousFixtures.Distinct().ToList();
            return previousFixtures;
        }

        public List<Fixture> GetPreviousResults()
        {
            return database.GetPreviousResults();
        }

        public void LoadDatabase()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Database.bpdb"; // Default file name
            dlg.DefaultExt = ".bpdb"; // Default file extension
            dlg.Filter = "Betting Predictor Database (.dpdb)|*.bpdb"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                using (FileStream fs = new FileStream(dlg.FileName, FileMode.Open))
                {
                    try
                    {
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        database = (Database)binaryFormatter.Deserialize(fs);
                    }
                    catch (SerializationException ex)
                    {
                        MessageBox.Show("Failed to deserialize. Reason: " + ex.Message);
                    }
                }
            }
        }
        public void SaveDatabase()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Database"; // Default file name
            dlg.DefaultExt = ".bpdb"; // Default file extension
            dlg.Filter = "Betting Predictor Database (.dpdb)|*.bpdb"; // Filter files by extension
            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                using (FileStream fs = new FileStream(dlg.FileName, FileMode.Create))
                {
                    try
                    {
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        binaryFormatter.Serialize(fs, database);
                    }
                    catch (SerializationException ex)
                    {
                        MessageBox.Show("Failed to serialize. Reason: " + ex.Message);
                    }
                }
            }
        }

        public void CreateBet()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "BET SLIP"; // Default file name
            dlg.DefaultExt = ".csv"; // Default file extension
            dlg.Filter = "CSV (Comma delimited) (.csv)|*.csv"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                var csvRows = new[] { new {
                    FixtureDate = DateTime.Now,
                    LeagueID = "LeagueID",
                    TeamName = "Team Name",
                    KellyCriterion = 0.0,
                    Bookie = "Bookmaker",
                    BestOdds = 0.0 } }.ToList();

                const double threashold = 0.01;
                var homeFixtures = database.FixtureList.Where(x => x.KellyCriterionHome > threashold);
                csvRows.AddRange(homeFixtures.Select(x => new
                {
                    FixtureDate = x.Date,
                    LeagueID = x.LeagueID,
                    TeamName = x.HomeTeam.Name,
                    KellyCriterion = x.KellyCriterionHome,
                    Bookie = x.BestHomeOdds.Name,
                    BestOdds = x.BestHomeOdds.HomeOdds
                }));

                var awayFixtures = database.FixtureList.Where(x => x.KellyCriterionAway > threashold);
                csvRows.AddRange(awayFixtures.Select(x => new
                {
                    FixtureDate = x.Date,
                    LeagueID = x.LeagueID,
                    TeamName = x.AwayTeam.Name,
                    KellyCriterion = x.KellyCriterionAway,
                    Bookie = x.BestAwayOdds.Name,
                    BestOdds = x.BestAwayOdds.AwayOdds
                }));

                csvRows.RemoveAt(0); // remove dummy anonymous object

                CsvDefinition csvDefinition = new CsvDefinition();
                csvDefinition.FieldSeparator = ',';
                csvDefinition.Columns = new List<string>{
                    "FixtureDate",
                    "LeagueID",
                    "TeamName",
                    "KellyCriterion",
                    "Bookie",
                    "BestOdds" };

                try
                {
                    csvRows.ToCsv(dlg.FileName, csvDefinition);
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public IEnumerable<ProfitLossInterval> CalculateHomeProfitLossIntervals()
        {
            return database.CalculateProfitLossIntervals().Where(x => x.HomeOrAway == "Home");
        }

        public IEnumerable<ProfitLossInterval> CalculateAwayProfitLossIntervals()
        {
            return database.CalculateProfitLossIntervals().Where(x => x.HomeOrAway == "Away");
        }
    }
}
