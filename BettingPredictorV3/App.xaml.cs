﻿using System;
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
            var dialogResult = OpenDatabaseSettingsWindow();
            if (dialogResult == true)
            {
                if (DatabaseSettings.PopulateDatabase)
                {
                    PopulateDatabase(splashWindow);

                    PredictResults();
                }

                // Create the main window, but on the UI thread.
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Invoker)delegate
                {
                    MainWindowViewModel viewModel = new MainWindowViewModel(database);
                    MainWindow = new MainWindow(viewModel);
                    MainWindow.Show();
                });
            }
        }

        private void PopulateDatabase(Splash splashWindow)
        {
            FileParser fileParser = new FileParser();
            database.ClearData();

            var csvFixtures = fileParser.LoadUpcomingFixturesFile(database.FixtureFiles);
            var upcomingFixtures = database.AddFixtures(csvFixtures.ToList<IDatabaseFixture>());
            database.FixtureList = upcomingFixtures;

            var relevantFiles = database.HistoryFiles.Where(x => (database.LeagueCodes.Find(y => y == x.Key) != null));
            var historicFixtures = fileParser.ParseFiles(splashWindow, relevantFiles);
            database.AddFixtures(historicFixtures.ToList<IDatabaseFixture>());
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
