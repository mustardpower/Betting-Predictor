using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.IO;
using CsvFiles;
using System.ComponentModel;
using BettingPredictorV3.DataStructures;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace BettingPredictorV3
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Database database = new Database();
        private BackgroundWorker worker = new BackgroundWorker();

        public MainWindow(Database aDatabase)
        { 
            InitializeComponent();
            database = aDatabase;
        }

        public List<Fixture> GetDefaultUpcomingFixtures()
        {
            List<Fixture> upcomingFixtures = database.GetUpcomingFixtures();
            // remove teams with less than a season of results
            upcomingFixtures.RemoveAll(x => database.GetFixturesBefore(x.HomeTeam, DateTime.Now).Count < 19);
            upcomingFixtures.RemoveAll(x => database.GetFixturesBefore(x.AwayTeam, DateTime.Now).Count < 19);
            return upcomingFixtures;
        }

        public void RefreshUpcomingFixturesTab()
        {
            var upcomingFixtures = GetDefaultUpcomingFixtures();
            dataGrid_UpcomingFixtures.ItemsSource = upcomingFixtures;
            leaguesComboBox.ItemsSource = database.Leagues;
            dateComboBox.ItemsSource = upcomingFixtures.Select(x => x.Date.DayOfYear).Distinct().Select(dayOfYear => new DateTime(DateTime.Now.Year, 1, 1).AddDays(dayOfYear - 1));
        }

        public void DataGrid_UpcomingFixtures_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshUpcomingFixturesTab();
        }

        private void DataGrid_PreviousFixtures_Loaded(object sender, RoutedEventArgs e)
        {
            //List<Fixture> previousFixtures = database.GetPreviousResults();
            //previousFixtures.RemoveAll(x => x.HomeTeam.GetFixturesBefore(x.Date).Count < 10);
            //previousFixtures.RemoveAll(x => x.AwayTeam.GetFixturesBefore(x.Date).Count < 10);
            //previousFixtures = previousFixtures.Distinct().ToList();
            //dataGrid_PreviousFixtures.ItemsSource = previousFixtures;

            //const float minValue = -3.0f;
            //const float maxValue = 3.0f;
            //const int noOfIntervals = 60;
            //database.CalculateProfitIntervals(previousFixtures, minValue, maxValue, noOfIntervals);

        }

        private void DataGrid_HomeProfitLossReport_Loaded(object sender, RoutedEventArgs e)
        {
            dataGrid_HomeProfitLossReport.ItemsSource = database.CalculateProfitLossIntervals().Where(x => x.HomeOrAway == "Home");
        }

        private void DataGrid_AwayProfitLossReport_Loaded(object sender, RoutedEventArgs e)
        {
            dataGrid_AwayProfitLossReport.ItemsSource = database.CalculateProfitLossIntervals().Where(x => x.HomeOrAway == "Away");
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Fixture selectedFixture;
            if (tabItem1.IsSelected)
            {
                selectedFixture = (Fixture)dataGrid_UpcomingFixtures.SelectedItem;
            }
            else if (tabItem2.IsSelected)
            {
                selectedFixture = (Fixture)dataGrid_PreviousFixtures.SelectedItem;
            }
            else
            {
                return;
            }

            var homeFixtures = database.GetFixturesBefore(selectedFixture.HomeTeam, DateTime.Now);
            var awayFixtures = database.GetFixturesBefore(selectedFixture.HomeTeam, DateTime.Now);

            FixtureWindow fixtureDlg = new FixtureWindow(selectedFixture, homeFixtures, awayFixtures);
            fixtureDlg.Show();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            List<Fixture> queriedFixtures = new List<Fixture>();
            if (tabItem1.IsSelected)
            {
                queriedFixtures = database.GetUpcomingFixtures();
                IEnumerable<String> leagueIDs = database.Leagues.Select(x => x.LeagueCode);

                if (leaguesComboBox.SelectedItem != null)
                {
                    queriedFixtures = queriedFixtures.Where(x => x.FixtureLeague == leaguesComboBox.SelectedItem).ToList();
                }

                if (dateComboBox.SelectedItem != null)
                {
                    DateTime selectedDate = (DateTime)(dateComboBox.SelectedItem);
                    queriedFixtures = queriedFixtures.Where(x => x.Date.DayOfYear == selectedDate.DayOfYear).ToList();
                }

                queriedFixtures = FilterForChosenGD(queriedFixtures);
                dataGrid_UpcomingFixtures.ItemsSource = queriedFixtures;
            }
            else if (tabItem2.IsSelected)
            {
                List<Fixture> previousFixtures = database.GetPreviousResults();
                previousFixtures.RemoveAll(x => database.GetFixturesBefore(x.HomeTeam, x.Date).Count < 10);
                previousFixtures.RemoveAll(x => database.GetFixturesBefore(x.AwayTeam, x.Date).Count < 10);
                queriedFixtures = previousFixtures.Distinct().ToList();
  
                IEnumerable<String> leagueIDs = database.Leagues.Select(x => x.LeagueCode);

                if (leaguesComboBox.SelectedItem != null)
                {
                    queriedFixtures = queriedFixtures.Where(x => x.FixtureLeague == leaguesComboBox.SelectedItem).ToList();
                }

                if (dateComboBox.SelectedItem != null)
                {
                    DateTime selectedDate = (DateTime)(dateComboBox.SelectedItem);
                    queriedFixtures = queriedFixtures.Where(x => x.Date.DayOfYear == selectedDate.DayOfYear).ToList();
                }

                queriedFixtures = FilterForChosenGD(queriedFixtures);
                dataGrid_PreviousFixtures.ItemsSource = queriedFixtures;
                Database.CalculateHomeGameProfit(queriedFixtures);
                Database.CalculateAwayGameProfit(queriedFixtures);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            List<Fixture> upcomingFixtures = new List<Fixture>();
            dateComboBox.SelectedItem = null;
            leaguesComboBox.SelectedItem = null;
            upcomingFixtures = database.GetUpcomingFixtures();

            // remove teams with less than a season of results
            upcomingFixtures.RemoveAll(x => database.GetFixturesBefore(x.HomeTeam, DateTime.Now).Count < 19);
            upcomingFixtures.RemoveAll(x => database.GetFixturesBefore(x.AwayTeam, DateTime.Now).Count < 19);

            dataGrid_UpcomingFixtures.ItemsSource = upcomingFixtures;
        }

        public List<Fixture> FilterForChosenGD(IEnumerable<Fixture> aFixtureList)
        {
            if (!((minGD.Text == null) || (minGD.Text.Equals(""))))
            {
                double minimumGD = Convert.ToDouble(minGD.Text);
                aFixtureList = aFixtureList.Where(x => x.PredictedGoalDifference > minimumGD);
            }
            if (!((maxGD.Text == null) || (maxGD.Text.Equals(""))))
            {
                double maximumGD = Convert.ToDouble(maxGD.Text);
                aFixtureList = aFixtureList.Where(x => x.PredictedGoalDifference < maximumGD);
            }

            return aFixtureList.ToList();
        }

        private void CreateBet(object sender, RoutedEventArgs e)
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
                var homeFixtures = database.GetUpcomingFixtures().Where(x => x.KellyCriterionHome > threashold);
                csvRows.AddRange(homeFixtures.Select(x => new {
                    FixtureDate = x.Date,
                    LeagueID = x.FixtureLeague.LeagueCode,
                    TeamName = x.HomeTeam.Name,
                    KellyCriterion = x.KellyCriterionHome,
                    Bookie = x.BestHomeOdds.Name,
                    BestOdds = x.BestHomeOdds.HomeOdds }));

                var awayFixtures = database.GetUpcomingFixtures().Where(x => x.KellyCriterionAway > threashold);
                csvRows.AddRange(awayFixtures.Select(x => new {
                    FixtureDate = x.Date,
                    LeagueID = x.FixtureLeague.LeagueCode,
                    TeamName = x.AwayTeam.Name,
                    KellyCriterion = x.KellyCriterionAway,
                    Bookie = x.BestAwayOdds.Name,
                    BestOdds = x.BestAwayOdds.AwayOdds }));

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
                catch(IOException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void LoadDatabase(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Database.bpdb"; // Default file name
            dlg.DefaultExt = ".bpdb"; // Default file extension
            dlg.Filter = "Betting Predictor Database (.dpdb)|*.bpdb"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            if(result == true)
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

                    RefreshUpcomingFixturesTab();
                }
            }
        }

        private void SaveDatabase(object sender, RoutedEventArgs e)
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

        private void ShowAbout(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("© Copyright of Paul Gothard. Not for commercial use.");
        }

        private void PredictResults()
        {
            double alpha, beta;

            do
            {
                alpha = database.GetAlphaValue();
                beta = database.GetBetaValue();
                database.PredictResults(alpha, beta);
            }
            while ((Math.Abs(alpha) > Math.Abs(database.GetAlphaValue()) && (Math.Abs(beta) > Math.Abs(database.GetBetaValue()))));


            List<double> homeResiduals = database.GetHomeResiduals(DateTime.Now);
            List<double> awayResiduals = database.GetAwayResiduals(DateTime.Now);

            homeResiduals.RemoveAll(x => Double.IsNaN(x));
            awayResiduals.RemoveAll(x => Double.IsNaN(x));
        }
    }
}
