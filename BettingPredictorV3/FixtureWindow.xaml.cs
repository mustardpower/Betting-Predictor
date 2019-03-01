using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;
using BettingPredictorV3.DataStructures;

namespace BettingPredictorV3
{
    /// <summary>
    /// Interaction logic for FixtureWindow.xaml
    /// </summary>
    public partial class FixtureWindow : Window
    {
        private Fixture selectedFixture;
        public FixtureWindow(Fixture aFixture)
        {
            InitializeComponent();
            selectedFixture = aFixture;
            CalculateResultProbabilities();
        }

        private void HomeTeamFixtures_Loaded(object sender, RoutedEventArgs e)
        {
            if(selectedFixture != null)
            {
                homeFixturesGrid.ItemsSource = selectedFixture.HomeTeam.Fixtures;
            }
        }

        private void AwayTeamFixtures_Loaded(object sender, RoutedEventArgs e)
        {
            if (selectedFixture != null)
            {
                awayFixturesGrid.ItemsSource = selectedFixture.AwayTeam.Fixtures;
            }
        }

        private void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if ((e.ColumnIndex == 11) || (e.ColumnIndex == 12))
            {
                if ((int)e.Value < 3)
                    e.CellStyle.BackColor = System.Drawing.Color.Blue;
                if ((int)e.Value > 2)
                    e.CellStyle.BackColor = System.Drawing.Color.Red;
            }
        }
        
        public void CalculateResultProbabilities()
        {
            if (selectedFixture != null)
            {
                homeProbability.Content = Math.Round(selectedFixture.HomeWinProbability,4);
                drawProbability.Content = Math.Round(selectedFixture.DrawProbability, 4);
                awayProbability.Content = Math.Round(selectedFixture.AwayWinProbability, 4);
            }
        }
    }
}
