using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BettingPredictorV3.DataStructures;

namespace BettingPredictorV3
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
    
        private MainWindowViewModel viewModel;

        public MainWindow(Database aDatabase)
        { 
            InitializeComponent();

            viewModel = new MainWindowViewModel(aDatabase);
        }

        public void RefreshUpcomingFixturesTab()
        {
            dataGrid_UpcomingFixtures.ItemsSource = viewModel.GetDefaultUpcomingFixtures();
            leaguesComboBox.ItemsSource = viewModel.Leagues;
            dateComboBox.ItemsSource = viewModel.UpcomingFixtureDates;
        }

        public void DataGrid_UpcomingFixtures_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshUpcomingFixturesTab();
        }

        private void DataGrid_PreviousFixtures_Loaded(object sender, RoutedEventArgs e)
        {
            dataGrid_PreviousFixtures.ItemsSource = viewModel.GetPreviousFixtures();
        }

        private void DataGrid_HomeProfitLossReport_Loaded(object sender, RoutedEventArgs e)
        {
            dataGrid_HomeProfitLossReport.ItemsSource = viewModel.CalculateHomeProfitLossIntervals();
        }

        private void DataGrid_AwayProfitLossReport_Loaded(object sender, RoutedEventArgs e)
        {
            dataGrid_AwayProfitLossReport.ItemsSource = viewModel.CalculateAwayProfitLossIntervals();
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
                queriedFixtures = viewModel.FixtureList.ToList();
                IEnumerable<String> leagueIDs = viewModel.Leagues.Select(x => x.LeagueID);

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
                List<Fixture> previousFixtures = viewModel.GetPreviousResults();
                previousFixtures.RemoveAll(x => x.HomeTeam.GetFixturesBefore(x.Date).Count < 10);
                previousFixtures.RemoveAll(x => x.AwayTeam.GetFixturesBefore(x.Date).Count < 10);
                queriedFixtures = previousFixtures.Distinct().ToList();

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
            upcomingFixtures = viewModel.FixtureList.ToList();

            // remove teams with less than a season of results
            upcomingFixtures.RemoveAll(x => x.HomeTeam.GetFixturesBefore(DateTime.Now).Count < 19);
            upcomingFixtures.RemoveAll(x => x.AwayTeam.GetFixturesBefore(DateTime.Now).Count < 19);

            dataGrid_UpcomingFixtures.ItemsSource = upcomingFixtures;
        }

        public List<Fixture> FilterForChosenGD(IEnumerable<Fixture> aFixtureList)
        {
            if (!(string.IsNullOrEmpty(minGD.Text)))
            {
                double minimumGD = Convert.ToDouble(minGD.Text);
                aFixtureList = aFixtureList.Where(x => x.PredictedGoalDifference > minimumGD);
            }
            if (!(string.IsNullOrEmpty(maxGD.Text)))
            {
                double maximumGD = Convert.ToDouble(maxGD.Text);
                aFixtureList = aFixtureList.Where(x => x.PredictedGoalDifference < maximumGD);
            }

            return aFixtureList.ToList();
        }

        private void CreateBet(object sender, RoutedEventArgs e)
        {
            viewModel.CreateBet();
        }

        private void LoadDatabase(object sender, RoutedEventArgs e)
        {
            viewModel.LoadDatabase();
            RefreshUpcomingFixturesTab();
        }

        private void SaveDatabase(object sender, RoutedEventArgs e)
        {
            viewModel.SaveDatabase();
        }
        

        private void ShowAbout(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("© Copyright of Paul Gothard. Not for commercial use.");
        }
    }
}
