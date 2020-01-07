using BettingPredictorV3.DataStructures;
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
    class MainWindowViewModel
    {
        private Database database = new Database();

        public MainWindowViewModel(Database aDatabase)
        {
            database = aDatabase;
        }

        public List<Fixture> GetDefaultUpcomingFixtures()
        {
            List<Fixture> upcomingFixtures = new List<Fixture>();
            upcomingFixtures = database.FixtureList;
            // remove teams with less than a season of results
            upcomingFixtures.RemoveAll(x => x.HomeTeam.GetFixturesBefore(DateTime.Now).Count < 19);
            upcomingFixtures.RemoveAll(x => x.AwayTeam.GetFixturesBefore(DateTime.Now).Count < 19);
            return upcomingFixtures;
        }

        public List<Fixture> GetPreviousFixtures()
        {
            List<Fixture> previousFixtures = database.GetPreviousResults();
            previousFixtures.RemoveAll(x => x.HomeTeam.GetFixturesBefore(x.Date).Count < 10);
            previousFixtures.RemoveAll(x => x.AwayTeam.GetFixturesBefore(x.Date).Count < 10);
            previousFixtures = previousFixtures.Distinct().ToList();
            return previousFixtures;
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
    }
}
