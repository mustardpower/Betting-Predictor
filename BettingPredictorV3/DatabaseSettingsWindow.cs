using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BettingPredictorV3
{
    public partial class DatabaseSettingsWindow : Form
    {
        public DatabaseSettingsWindow()
        {
            InitializeComponent();

            if(DatabaseSettings.BookmakersUsed.IndexOf("Bet 365") > -1) { bet365Checkbox.Checked = true; }
            if (DatabaseSettings.BookmakersUsed.IndexOf("InterWetten") > -1) { interwettenCheckbox.Checked = true; }
            if (DatabaseSettings.BookmakersUsed.IndexOf("BetWin") > -1) { betWinCheckbox.Checked = true; }
            if (DatabaseSettings.BookmakersUsed.IndexOf("William Hill") > -1) { williamHillCheckbox.Checked = true; }
            if (DatabaseSettings.BookmakersUsed.IndexOf("Victor Chandler") > -1) { victorChandlerCheckbox.Checked = true; }
            if (DatabaseSettings.BookmakersUsed.IndexOf("Pinnacle Sport") > -1) { pinnacleSportCheckbox.Checked = true; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> bookmakersSelected = new List<string>();

            if (bet365Checkbox.CheckState == CheckState.Checked) { bookmakersSelected.Add("Bet 365"); }
            if (interwettenCheckbox.CheckState == CheckState.Checked) { bookmakersSelected.Add("InterWetten"); }
            if (betWinCheckbox.CheckState == CheckState.Checked) { bookmakersSelected.Add("BetWin"); }
            if (victorChandlerCheckbox.CheckState == CheckState.Checked) { bookmakersSelected.Add("Victor Chandler"); }
            if (williamHillCheckbox.CheckState == CheckState.Checked) { bookmakersSelected.Add("William Hill"); }
            if (pinnacleSportCheckbox.CheckState == CheckState.Checked) { bookmakersSelected.Add("Pinnacle Sport"); }

            DatabaseSettings.BookmakersUsed = bookmakersSelected;
            DatabaseSettings.PopulateDatabase = populateDatabaseCheckBox.Checked;
            DatabaseSettings.IgnorePlayedFixtures = ignoredPlayedFixturesCheckbox.Checked;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancel(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
