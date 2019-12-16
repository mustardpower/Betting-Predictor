using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BettingPredictorV3
{
    /// <summary>
    /// Interaction logic for DatabaseSettings.xaml
    /// </summary>
    public partial class DatabaseSettingsWindow : Window
    {

        public DatabaseSettingsWindow()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            List<string> bookmakersSelected = new List<string>();

            if (bet365Checkbox.IsChecked ?? false) { bookmakersSelected.Add("Bet 365"); }
            if (interwettenCheckbox.IsChecked ?? false) { bookmakersSelected.Add("InterWetten"); }
            if (betWinCheckbox.IsChecked ?? false) { bookmakersSelected.Add("BetWin"); }
            if (victorChandlerCheckbox.IsChecked ?? false) { bookmakersSelected.Add("Victor Chandler"); }
            if (williamHillCheckbox.IsChecked ?? false) { bookmakersSelected.Add("William Hill"); }
            if (pinnacleSportCheckbox.IsChecked ?? false) { bookmakersSelected.Add("Pinnacle Sport"); }

            DatabaseSettings.BookmakersUsed = bookmakersSelected;
            DatabaseSettings.PopulateDatabase = populateDatabaseCheckBox.IsChecked ?? false;
            DatabaseSettings.IgnorePlayedFixtures = ignoredPlayedFixturesCheckbox.IsChecked ?? false;

            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
