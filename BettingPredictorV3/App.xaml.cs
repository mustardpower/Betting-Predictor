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
        private Database database;
        
        public App()
        {
            ApplicationInitialize = _applicationInitialize;
        }
        public static new App Current
        {
            get { return Application.Current as App; }
        }
        internal delegate void ApplicationInitializeDelegate(Splash splashWindow);
        internal ApplicationInitializeDelegate ApplicationInitialize;
        private void _applicationInitialize(Splash splashWindow)
        {
            database = new Database();
            database.SetFixturesFiles();
            database.SetHistoryFiles();
            var dialogResult = OpenDatabaseSettingsWindow();
            if (dialogResult == true)
            {
                if (DatabaseSettings.PopulateDatabase)
                {
                    FileParser fileParser = new FileParser();
                    fileParser.PopulateDatabase(database, splashWindow);
                }

                PredictResults();

                // Create the main window, but on the UI thread.
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Invoker)delegate
                {
                    MainWindow = new MainWindow(database);
                    MainWindow.Show();
                    MainWindow.Closed += MainWindow_Closed;
                });
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            database?.Dispose();
        }

        private bool? OpenDatabaseSettingsWindow()
        {
            DatabaseSettingsWindow databaseSettingsWindow = null;
            bool? dialogResult = false;
            ManualResetEvent m = new ManualResetEvent(false);
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Invoker)delegate
            {
                databaseSettingsWindow = new DatabaseSettingsWindow();
                databaseSettingsWindow.ShowDialog();
                dialogResult = databaseSettingsWindow.DialogResult;
                m.Set();
            });

            m.WaitOne();
            return dialogResult;
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
        }
    }
}
