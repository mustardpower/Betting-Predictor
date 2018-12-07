using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.IO;
using CsvFiles;
using System.ComponentModel;

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
            List<Fixture> upcoming_fixtures = new List<Fixture>();     
            upcoming_fixtures = database.FixtureList;
            // remove teams with less than a season of results
            upcoming_fixtures.RemoveAll(x => x.HomeTeam.GetFixturesBefore(DateTime.Now).Count < 19);
            upcoming_fixtures.RemoveAll(x => x.AwayTeam.GetFixturesBefore(DateTime.Now).Count < 19);

            dataGrid_UpcomingFixtures.ItemsSource = upcoming_fixtures;
            leaguesComboBox.ItemsSource = database.Leagues;
            dateComboBox.ItemsSource = upcoming_fixtures.Select(x => x.date.DayOfYear).Distinct().Select(dayOfYear => new DateTime(DateTime.Now.Year, 1, 1).AddDays(dayOfYear - 1));
        }

        private void DataGrid_PreviousFixtures_Loaded(object sender, RoutedEventArgs e)
        {
            List<Fixture> previous_fixtures = database.GetPreviousResults();
            previous_fixtures.RemoveAll(x => x.HomeTeam.GetFixturesBefore(x.date).Count < 10);
            previous_fixtures.RemoveAll(x => x.AwayTeam.GetFixturesBefore(x.date).Count < 10);
            previous_fixtures = previous_fixtures.Distinct().ToList();
            dataGrid_PreviousFixtures.ItemsSource = previous_fixtures;

            float minValue = -3.0f;
            float maxValue = 3.0f;
            int noOfIntervals = 60;
            CalculateProfitIntervals(previous_fixtures,minValue, maxValue, noOfIntervals);

        }

        private void DataGrid_ProfitLossReport_Loaded(object sender, RoutedEventArgs e)
        {
            List<Fixture> previousFixtures = database.GetPreviousResults();
            float min = -3.0f;
            float max = 3.0f;
            int numberOfSteps = 40;
            List<ProfitLossInterval> profitLossIntervals = CalculateProfitIntervals(previousFixtures, min, max, numberOfSteps);
            dataGrid_ProfitLossReport.ItemsSource = profitLossIntervals;
        }

        public List<ProfitLossInterval> CalculateProfitIntervals(List<Fixture> previousFixtures, float min, float max, int n)
        {
            float h = ((max-min)/n); // step size of each interval
            float x1 = min;
            float x2 = x1 + h;
            List<Fixture> intervalFixtures = new List<Fixture>();
            List<ProfitLossInterval> profitLossIntervals = new List<ProfitLossInterval>();
            for (int i = 0; i < n; i++)
            {
                intervalFixtures = FilterForChosenGD(previousFixtures, x1, x2);
                ProfitLossInterval homeInterval = CalculateHomeGameProfit(intervalFixtures);
                ProfitLossInterval awayInterval = CalculateAwayGameProfit(intervalFixtures);
                x1 = x2;
                x2 += h;

                homeInterval.setRange(x1, x2);
                awayInterval.setRange(x1, x2);
                profitLossIntervals.Add(homeInterval);
                profitLossIntervals.Add(awayInterval);
            }

            return profitLossIntervals;
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
                    queriedFixtures = queriedFixtures.Where(x => x.league == leaguesComboBox.SelectedItem).ToList();
                }

                if (dateComboBox.SelectedItem != null)
                {
                    DateTime selectedDate = (DateTime)(dateComboBox.SelectedItem);
                    queriedFixtures = queriedFixtures.Where(x => x.date.DayOfYear == selectedDate.DayOfYear).ToList();
                }

                queriedFixtures = FilterForChosenGD(queriedFixtures);
                dataGrid_UpcomingFixtures.ItemsSource = queriedFixtures;
            }
            else if (tabItem2.IsSelected)
            {
                List<Fixture> previous_fixtures = database.GetPreviousResults();
                previous_fixtures.RemoveAll(x => x.HomeTeam.GetFixturesBefore(x.date).Count < 10);
                previous_fixtures.RemoveAll(x => x.AwayTeam.GetFixturesBefore(x.date).Count < 10);
                queriedFixtures = previous_fixtures.Distinct().ToList();
  
                IEnumerable<String> leagueIDs = database.Leagues.Select(x => x.LeagueID);

                if (leaguesComboBox.SelectedItem != null)
                {
                    queriedFixtures = queriedFixtures.Where(x => x.league == leaguesComboBox.SelectedItem).ToList();
                }

                if (dateComboBox.SelectedItem != null)
                {
                    DateTime selectedDate = (DateTime)(dateComboBox.SelectedItem);
                    queriedFixtures = queriedFixtures.Where(x => x.date.DayOfYear == selectedDate.DayOfYear).ToList();
                }

                queriedFixtures = FilterForChosenGD(queriedFixtures);
                dataGrid_PreviousFixtures.ItemsSource = queriedFixtures;
                CalculateHomeGameProfit(queriedFixtures);
                CalculateAwayGameProfit(queriedFixtures);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            List<Fixture> upcoming_fixtures = new List<Fixture>();
            dateComboBox.SelectedItem = null;
            leaguesComboBox.SelectedItem = null;
            upcoming_fixtures = database.FixtureList;

            // remove teams with less than a season of results
            upcoming_fixtures.RemoveAll(x => x.HomeTeam.GetFixturesBefore(DateTime.Now).Count < 19);
            upcoming_fixtures.RemoveAll(x => x.AwayTeam.GetFixturesBefore(DateTime.Now).Count < 19);

            dataGrid_UpcomingFixtures.ItemsSource = upcoming_fixtures;
        }

        public List<Fixture> FilterForChosenGD(IEnumerable<Fixture> aFixtureList)
        {
            if (!((minGD.Text == null) || (minGD.Text.Equals(""))))
            {
                double minimumGD = Convert.ToDouble(minGD.Text);
                aFixtureList = aFixtureList.Where(x => x.predicted_goal_difference > minimumGD);
            }
            if (!((maxGD.Text == null) || (maxGD.Text.Equals(""))))
            {
                double maximumGD = Convert.ToDouble(maxGD.Text);
                aFixtureList = aFixtureList.Where(x => x.predicted_goal_difference < maximumGD);
            }

            return aFixtureList.ToList();
        }

        public List<Fixture> FilterForChosenGD(IEnumerable<Fixture> aFixtureList, double minGD, double maxGD)
        {
            aFixtureList = aFixtureList.Where(x => x.predicted_goal_difference > minGD);
            aFixtureList = aFixtureList.Where(x => x.predicted_goal_difference < maxGD);

            return aFixtureList.ToList();
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

        private ProfitLossInterval CalculateHomeGameProfit(List<Fixture> fixtures)
        {
            double profit = 0.0;
            int ignoredTeams = 0;
            Fixture max_odds_fixture = new Fixture();
            max_odds_fixture.best_home_odds = new Bookmaker("bookie",0.0,0.0,0.0);

            foreach(Fixture fixture in fixtures)
            {
                if (fixture.best_home_odds == null)
                {
                    ignoredTeams++;
                }
                else
                {
                    if (fixture.HomeGoals > fixture.AwayGoals)
                    {
                        if (max_odds_fixture.best_home_odds.HomeOdds < fixture.best_home_odds.HomeOdds)
                        {
                            max_odds_fixture = fixture;
                        }
                        profit += fixture.best_home_odds.HomeOdds - 1;
                    }
                    else
                    {
                        profit--;
                    }
                }
            }

            double yield = profit / fixtures.Count;
            string intervalName = "Test interval name";
            return new ProfitLossInterval(intervalName, "Home", fixtures.Count, profit, yield);
        }

        private ProfitLossInterval CalculateAwayGameProfit(List<Fixture> fixtures)
        {
            double profit = 0.0;
            int ignoredTeams = 0;

            foreach (Fixture fixture in fixtures)
            {
                if (fixture.best_away_odds == null)
                {
                    ignoredTeams++;
                }
                else
                {
                    if (fixture.HomeGoals < fixture.AwayGoals)
                    {
                        profit += (fixture.best_away_odds.AwayOdds - 1);
                    }
                    else
                    {
                        profit--;
                    }
                }
            }

            double yield = profit / fixtures.Count;
            string intervalName = "Test interval name";
            return new ProfitLossInterval(intervalName, "Away", fixtures.Count, profit, yield);
        }

        private void ShowAbout(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("© Copyright of Paul Gothard. Not for commercial use.");
        }

        private void PredictResults()
        {
            double alpha, beta;

            List<double> errors = new List<double>();
            List<double> beta_values = new List<double>();

            do
            {
                alpha = database.GetAlphaValue();
                beta = database.GetBetaValue();
                database.PredictResults(alpha, beta);
            }
            while ((Math.Abs(alpha) > Math.Abs(database.GetAlphaValue()) && (Math.Abs(beta) > Math.Abs(database.GetBetaValue()))));


            List<double> home_residuals = database.GetHomeResiduals(DateTime.Now);
            List<double> away_residuals = database.GetAwayResiduals(DateTime.Now);

            home_residuals.RemoveAll(x => Double.IsNaN(x));
            double home_average_error = home_residuals.Average();
            away_residuals.RemoveAll(x => Double.IsNaN(x));
            double away_average_error = away_residuals.Average();
        }
    }
}
