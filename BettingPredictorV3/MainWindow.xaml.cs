using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.IO;
using CsvFiles;
using System.ComponentModel;
using BettingPredictorV3.DataStructures;

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

        public void DataGrid_UpcomingFixtures_Loaded(object sender, RoutedEventArgs e)
        {
            List<Fixture> upcomingFixtures = new List<Fixture>();
            upcomingFixtures = database.FixtureList;
            // remove teams with less than a season of results
            upcomingFixtures.RemoveAll(x => x.HomeTeam.GetFixturesBefore(DateTime.Now).Count < 19);
            upcomingFixtures.RemoveAll(x => x.AwayTeam.GetFixturesBefore(DateTime.Now).Count < 19);

            dataGrid_UpcomingFixtures.ItemsSource = upcomingFixtures;
            leaguesComboBox.ItemsSource = database.Leagues;
            dateComboBox.ItemsSource = upcomingFixtures.Select(x => x.Date.DayOfYear).Distinct().Select(dayOfYear => new DateTime(DateTime.Now.Year, 1, 1).AddDays(dayOfYear - 1));
        }

        private void DataGrid_PreviousFixtures_Loaded(object sender, RoutedEventArgs e)
        {
            List<Fixture> previousFixtures = database.GetPreviousResults();
            previousFixtures.RemoveAll(x => x.HomeTeam.GetFixturesBefore(x.Date).Count < 10);
            previousFixtures.RemoveAll(x => x.AwayTeam.GetFixturesBefore(x.Date).Count < 10);
            previousFixtures = previousFixtures.Distinct().ToList();
            dataGrid_PreviousFixtures.ItemsSource = previousFixtures;

            const float minValue = -3.0f;
            const float maxValue = 3.0f;
            const int noOfIntervals = 60;
            database.CalculateProfitIntervals(previousFixtures, minValue, maxValue, noOfIntervals);

        }

        private void DataGrid_ProfitLossReport_Loaded(object sender, RoutedEventArgs e)
        {
            dataGrid_ProfitLossReport.ItemsSource = database.CalculateProfitLossIntervals();
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
            FixtureWindow fixtureDlg = new FixtureWindow(selectedFixture);
            fixtureDlg.Show();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            List<Fixture> queriedFixtures = new List<Fixture>();
            if (tabItem1.IsSelected)
            {
                queriedFixtures = database.FixtureList;
                IEnumerable<String> leagueIDs = database.Leagues.Select(x => x.LeagueID);

                if (leaguesComboBox.SelectedItem != null)
                {
                    queriedFixtures = queriedFixtures.Where(x => x.League == leaguesComboBox.SelectedItem).ToList();
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
                previousFixtures.RemoveAll(x => x.HomeTeam.GetFixturesBefore(x.Date).Count < 10);
                previousFixtures.RemoveAll(x => x.AwayTeam.GetFixturesBefore(x.Date).Count < 10);
                queriedFixtures = previousFixtures.Distinct().ToList();
  
                IEnumerable<String> leagueIDs = database.Leagues.Select(x => x.LeagueID);

                if (leaguesComboBox.SelectedItem != null)
                {
                    queriedFixtures = queriedFixtures.Where(x => x.League == leaguesComboBox.SelectedItem).ToList();
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
            upcomingFixtures = database.FixtureList;

            // remove teams with less than a season of results
            upcomingFixtures.RemoveAll(x => x.HomeTeam.GetFixturesBefore(DateTime.Now).Count < 19);
            upcomingFixtures.RemoveAll(x => x.AwayTeam.GetFixturesBefore(DateTime.Now).Count < 19);

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
                var csvRows = new[] { new { FixtureDate = DateTime.Now, LeagueID = "LeagueID", TeamName = "Team Name", KellyCriterion = 0.0} }.ToList();
                const double threashold = 0.01;
                var homeFixtures = database.FixtureList.Where(x => x.KellyCriterionHome > threashold);
                csvRows.AddRange(homeFixtures.Select(x => new { FixtureDate = x.Date, LeagueID = x.LeagueID, TeamName = x.HomeTeam.Name, KellyCriterion = x.KellyCriterionHome }));
                var awayFixtures = database.FixtureList.Where(x => x.KellyCriterionAway > threashold);
                csvRows.AddRange(awayFixtures.Select(x => new { FixtureDate = x.Date, LeagueID = x.LeagueID, TeamName = x.AwayTeam.Name, KellyCriterion = x.KellyCriterionAway }));
                csvRows.RemoveAt(0); // remove dummy anonymous object

                CsvDefinition csvDefinition = new CsvDefinition();
                csvDefinition.FieldSeparator = ',';
                csvDefinition.Columns = new List<string>{ "FixtureDate", "LeagueID", "TeamName", "KellyCriterion" };

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

        private void GridToCSV(object sender, RoutedEventArgs e)
        {
            // get objects in the datagrid
            IEnumerable<Fixture> fixtures = (IEnumerable<Fixture>)dataGrid_UpcomingFixtures.ItemsSource;
            fixtures.ToCsv("TEST.csv");
        }

        private void CSVToGrid(object sender, RoutedEventArgs e)
        {
            // get objects in file and place in data grid

            try
            {
                dataGrid_UpcomingFixtures.ItemsSource = CsvFile.Read<Fixture>("TEST.csv");
            }
            catch(FileNotFoundException ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
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
