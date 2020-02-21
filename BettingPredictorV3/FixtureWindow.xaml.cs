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
        public List<Fixture> HomeFixtures { get; set; }
        public List<Fixture> AwayFixtures { get; set; }

        public FixtureWindow(Fixture aFixture, List<Fixture> homeTeamFixtures, List<Fixture> awayTeamFixtures)
        {
            InitializeComponent();
            HomeFixtures = homeTeamFixtures;
            AwayFixtures = awayTeamFixtures;
            DataContext = aFixture;
        }

        private void HomeTeamFixtures_Loaded(object sender, RoutedEventArgs e)
        {
            if(HomeFixtures != null)
            {
                homeFixturesGrid.ItemsSource = HomeFixtures;
            }
        }

        private void AwayTeamFixtures_Loaded(object sender, RoutedEventArgs e)
        {
            if (AwayFixtures != null)
            {
                awayFixturesGrid.ItemsSource = AwayFixtures;
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
    }
}
