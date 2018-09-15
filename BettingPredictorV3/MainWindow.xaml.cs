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

        public void dataGrid_UpcomingFixtures_Loaded(object sender, RoutedEventArgs e)
        {
            List<Fixture> upcoming_fixtures = new List<Fixture>();     
            upcoming_fixtures = database.getFixtures();
            // remove teams with less than a season of results
            upcoming_fixtures.RemoveAll(x => x.getHomeTeam().getFixturesBefore(DateTime.Now).Count < 19);
            upcoming_fixtures.RemoveAll(x => x.getAwayTeam().getFixturesBefore(DateTime.Now).Count < 19);

            dataGrid_UpcomingFixtures.ItemsSource = upcoming_fixtures;
            leaguesComboBox.ItemsSource = database.getLeagues();
            dateComboBox.ItemsSource = upcoming_fixtures.Select(x => x.date).Distinct();
        }

        private void dataGrid_PreviousFixtures_Loaded(object sender, RoutedEventArgs e)
        {
            List<Fixture> previous_fixtures = database.getPreviousResults();
            previous_fixtures.RemoveAll(x => x.getHomeTeam().getFixturesBefore(x.date).Count < 10);
            previous_fixtures.RemoveAll(x => x.getAwayTeam().getFixturesBefore(x.date).Count < 10);
            previous_fixtures = previous_fixtures.Distinct().ToList();
            dataGrid_PreviousFixtures.ItemsSource = previous_fixtures;

            float minValue = -3.0f;
            float maxValue = 3.0f;
            int noOfIntervals = 60;
            calculateProfitIntervals(previous_fixtures,minValue, maxValue, noOfIntervals);

        }

        private void dataGrid_ProfitLossReport_Loaded(object sender, RoutedEventArgs e)
        {
            List<Fixture> previousFixtures = database.getPreviousResults();
            float min = -3.0f;
            float max = 3.0f;
            int numberOfSteps = 40;
            List<ProfitLossInterval> profitLossIntervals = calculateProfitIntervals(previousFixtures, min, max, numberOfSteps);
            dataGrid_ProfitLossReport.ItemsSource = profitLossIntervals;
        }

        public List<ProfitLossInterval> calculateProfitIntervals(List<Fixture> previousFixtures, float min, float max, int n)
        {
            float h = ((max-min)/n); // step size of each interval
            float x1 = min;
            float x2 = x1 + h;
            List<Fixture> intervalFixtures = new List<Fixture>();
            List<ProfitLossInterval> profitLossIntervals = new List<ProfitLossInterval>();
            for (int i = 0; i < n; i++)
            {
                intervalFixtures = filterForChosenGD(previousFixtures, x1, x2);
                ProfitLossInterval homeInterval = calculateHomeGameProfit(intervalFixtures);
                ProfitLossInterval awayInterval = calculateAwayGameProfit(intervalFixtures);
                x1 = x2;
                x2 += h;

                homeInterval.setRange(x1, x2);
                awayInterval.setRange(x1, x2);
                profitLossIntervals.Add(homeInterval);
                profitLossIntervals.Add(awayInterval);
            }

            return profitLossIntervals;
        }

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            List<Fixture> queriedFixtures = new List<Fixture>();
            if (tabItem1.IsSelected)
            {
                queriedFixtures = database.getFixtures();
                IEnumerable<String> leagueIDs = database.getLeagues().Select(x => x.LeagueID);

                if (leaguesComboBox.SelectedItem != null)
                {
                    queriedFixtures = queriedFixtures.Where(x => x.league == leaguesComboBox.SelectedItem).ToList();
                }

                if (dateComboBox.SelectedItem != null)
                {
                    queriedFixtures = queriedFixtures.Where(x => x.date == (DateTime)dateComboBox.SelectedItem).ToList();
                }

                queriedFixtures = filterForChosenGD(queriedFixtures);
                dataGrid_UpcomingFixtures.ItemsSource = queriedFixtures;
            }
            else if (tabItem2.IsSelected)
            {
                List<Fixture> previous_fixtures = database.getPreviousResults();
                previous_fixtures.RemoveAll(x => x.getHomeTeam().getFixturesBefore(x.date).Count < 10);
                previous_fixtures.RemoveAll(x => x.getAwayTeam().getFixturesBefore(x.date).Count < 10);
                queriedFixtures = previous_fixtures.Distinct().ToList();
  
                IEnumerable<String> leagueIDs = database.getLeagues().Select(x => x.LeagueID);

                if (leaguesComboBox.SelectedItem != null)
                {
                    queriedFixtures = queriedFixtures.Where(x => x.league == leaguesComboBox.SelectedItem).ToList();
                }

                if (dateComboBox.SelectedItem != null)
                {
                    queriedFixtures = queriedFixtures.Where(x => x.date == (DateTime)dateComboBox.SelectedItem).ToList();
                }

                queriedFixtures = filterForChosenGD(queriedFixtures);
                dataGrid_PreviousFixtures.ItemsSource = queriedFixtures;
                calculateHomeGameProfit(queriedFixtures);
                calculateAwayGameProfit(queriedFixtures);
            }
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            List<Fixture> upcoming_fixtures = new List<Fixture>();
            dateComboBox.SelectedItem = null;
            leaguesComboBox.SelectedItem = null;
            upcoming_fixtures = database.getFixtures();

            // remove teams with less than a season of results
            upcoming_fixtures.RemoveAll(x => x.getHomeTeam().getFixturesBefore(DateTime.Now).Count < 19);
            upcoming_fixtures.RemoveAll(x => x.getAwayTeam().getFixturesBefore(DateTime.Now).Count < 19);

            dataGrid_UpcomingFixtures.ItemsSource = upcoming_fixtures;
        }

        public List<Fixture> filterForChosenGD(IEnumerable<Fixture> aFixtureList)
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

        public List<Fixture> filterForChosenGD(IEnumerable<Fixture> aFixtureList, double minGD, double maxGD)
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

        private ProfitLossInterval calculateHomeGameProfit(List<Fixture> fixtures)
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
                    if (fixture.home_goals > fixture.away_goals)
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

        private ProfitLossInterval calculateAwayGameProfit(List<Fixture> fixtures)
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
                    if (fixture.home_goals < fixture.away_goals)
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

        private void predictResults()
        {
            double alpha, beta;

            List<double> errors = new List<double>();
            List<double> beta_values = new List<double>();

            do
            {
                alpha = database.getAlphaValue();
                beta = database.getBetaValue();
                database.predictResults(alpha, beta);
            }
            while ((Math.Abs(alpha) > Math.Abs(database.getAlphaValue()) && (Math.Abs(beta) > Math.Abs(database.getBetaValue()))));


            List<double> home_residuals = database.getHomeResiduals(DateTime.Now);
            List<double> away_residuals = database.getAwayResiduals(DateTime.Now);

            home_residuals.RemoveAll(x => Double.IsNaN(x));
            double home_average_error = home_residuals.Average();
            away_residuals.RemoveAll(x => Double.IsNaN(x));
            double away_average_error = away_residuals.Average();
        }
    }
}
