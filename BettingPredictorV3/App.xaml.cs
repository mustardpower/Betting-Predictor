using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace BettingPredictorV3
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    internal delegate void Invoker();
    public partial class App : Application
    {
        private Database database = new Database();
        
        public App()
        {
            ApplicationInitialize = _applicationInitialize;
            database.SetFixturesFiles();
            database.SetHistoryFiles();
        }
        public static new App Current
        {
            get { return Application.Current as App; }
        }
        internal delegate void ApplicationInitializeDelegate(Splash splashWindow);
        internal ApplicationInitializeDelegate ApplicationInitialize;
        private void _applicationInitialize(Splash splashWindow)
        {
            new DatabaseSettingsWindow().ShowDialog();
            List<String> historyFiles = database.HistoryFiles;
            database.ClearData();
            int fileNumber = 0;
            double progress = 0.0;

            foreach (String file in historyFiles)          // download and parse previous results
            {
                database.LoadHistoricalFile(file);
                fileNumber++;
                progress = (double)fileNumber / (double)historyFiles.Count;
                splashWindow.SetProgress(progress);
                splashWindow.SetText(System.String.Format("Loading historical data file number: {0} / {1} File Name: {2}", fileNumber, historyFiles.Count, file));
            }

            splashWindow.SetText("Loading upcoming fixtures...");
            database.LoadUpcomingFixturesFile();
            splashWindow.SetText("Predicting upcoming fixtures...");
            PredictResults();
            // Create the main window, but on the UI thread.
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Invoker)delegate
            {
                MainWindow = new MainWindow(database);
                MainWindow.Show();
            });
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
