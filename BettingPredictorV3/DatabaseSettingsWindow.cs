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

            if(DatabaseSettings.bookmakersUsed.IndexOf("Bet 365") > -1) { bet365Checkbox.Checked = true; }
            if (DatabaseSettings.bookmakersUsed.IndexOf("InterWetten") > -1) { interwettenCheckbox.Checked = true; }
            if (DatabaseSettings.bookmakersUsed.IndexOf("BetWin") > -1) { betWinCheckbox.Checked = true; }
            if (DatabaseSettings.bookmakersUsed.IndexOf("Stan James") > -1) { stanJamesCheckbox.Checked = true; }
            if (DatabaseSettings.bookmakersUsed.IndexOf("William Hill") > -1) { williamHillCheckbox.Checked = true; }
            if (DatabaseSettings.bookmakersUsed.IndexOf("Ladbrokes") > -1) { ladbrokesCheckbox.Checked = true; }
            if (DatabaseSettings.bookmakersUsed.IndexOf("Pinnacle Sport") > -1) { pinnacleSportCheckbox.Checked = true; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> bookmakersSelected = new List<string>();

            if (bet365Checkbox.CheckState == CheckState.Checked) { bookmakersSelected.Add("Bet 365"); }
            if (interwettenCheckbox.CheckState == CheckState.Checked) { bookmakersSelected.Add("InterWetten"); }
            if (betWinCheckbox.CheckState == CheckState.Checked) { bookmakersSelected.Add("BetWin"); }
            if (stanJamesCheckbox.CheckState == CheckState.Checked) { bookmakersSelected.Add("Stan James"); }
            if (williamHillCheckbox.CheckState == CheckState.Checked) { bookmakersSelected.Add("William Hill"); }
            if (ladbrokesCheckbox.CheckState == CheckState.Checked) { bookmakersSelected.Add("Ladbrokes"); }
            if (pinnacleSportCheckbox.CheckState == CheckState.Checked) { bookmakersSelected.Add("Pinnacle Sport"); }

            DatabaseSettings.bookmakersUsed = bookmakersSelected;

            this.Close();
        }

        private void cancel(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
