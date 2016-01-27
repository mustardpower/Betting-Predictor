﻿using System;
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
            calculateResultProbabilities();
        }

        private void homeTeamFixtures_Loaded(object sender, RoutedEventArgs e)
        {
            if(selectedFixture != null)
            {
                homeFixturesGrid.ItemsSource = selectedFixture.home_team.Fixtures;
            }
        }

        private void awayTeamFixtures_Loaded(object sender, RoutedEventArgs e)
        {
            if (selectedFixture != null)
            {
                awayFixturesGrid.ItemsSource = selectedFixture.away_team.Fixtures;
            }
        }

        private void dataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if ((e.ColumnIndex == 11) || (e.ColumnIndex == 12))
            {
                if ((int)e.Value < 3)
                    e.CellStyle.BackColor = System.Drawing.Color.Blue;
                if ((int)e.Value > 2)
                    e.CellStyle.BackColor = System.Drawing.Color.Red;
            }
        }
        
        public void calculateResultProbabilities()
        {
            if (selectedFixture != null)
            {
                homeProbability.Content = Math.Round(selectedFixture.homeWinProbability(),4);
                drawProbability.Content = Math.Round(selectedFixture.drawProbability(), 4);
                awayProbability.Content = Math.Round(selectedFixture.awayWinProbability(), 4);
            }
        }
    }
}