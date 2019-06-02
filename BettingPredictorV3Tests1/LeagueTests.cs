using Microsoft.VisualStudio.TestTools.UnitTesting;
using BettingPredictorV3.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingPredictorV3.DataStructures.Tests
{
    [TestClass()]
    public class LeagueTests
    {
        [TestMethod()]
        public void ParseHistoricalDataTest()
        {
            League league = new League("SP2");

            string[] testData = new string[]
            {
                "SP2",
                "16 / 08 / 13",
                "Girona", "Alaves",
                "1", "0",
                "H", "0", "0", "D",
                "1.95", "3.3", "4", "1.8", "3.2", "4.33",
                "1.85", "3.2", "3.8", "1.72", "3.25", "4.33",
                "2.04", "3.36", "4.13", "1.91", "3.2", "4",
                "1.91", "3.4", "3.8", "2.05", "3.4", "4", "30",
                "2.05", "1.95", "3.41", "3.32", "4.35", "3.85",
                "27", "2.22", "2.13", "1.77", "1.7",
                "", "", "", "", "", "", "2.37", "3.19", "3.45"
            };

            league.ParseHistoricalData(testData);

            Assert.AreEqual(league.LeagueID, "SP2");
            Assert.AreEqual(league.Teams.Count, 2);

            Fixture fixture = league.GetFixtures().First();

            Assert.AreEqual(fixture.League, league);
            Assert.AreEqual(fixture.LeagueID, "SP2");
            Assert.AreEqual(fixture.Date.Day, 16);
            Assert.AreEqual(fixture.Date.Month, 8);
            Assert.AreEqual(fixture.Date.Year, 2013);

            Assert.AreEqual(fixture.HomeTeam.Name, "Girona");
            Assert.AreEqual(fixture.AwayTeam.Name, "Alaves");
            Assert.AreEqual(fixture.HomeGoals, 1);
            Assert.AreEqual(fixture.AwayGoals, 0);
        }

        [TestMethod()]
        public void ParseBookmakerOddsTest()
        {
            League league = new League("SP2");
            string[] testData = new string[]
            {
                "SP2",
                "16 / 08 / 13",
                "Girona", "Alaves",
                "1", "0",
                "H", "0", "0", "D",
                "1.95", "3.3", "4", "1.8", "3.2", "4.33",
                "1.85", "3.2", "3.8", "1.72", "3.25", "4.33",
                "2.04", "3.36", "4.13", "1.91", "3.2", "4",
                "1.91", "3.4", "3.8", "2.05", "3.4", "4", "30",
                "2.05", "1.95", "3.41", "3.32", "4.35", "3.85",
                "27", "2.22", "2.13", "1.77", "1.7",
                "", "", "", "", "", "", "2.37", "3.19", "3.45"
            };

            league.ParseHistoricalData(testData);
            Fixture fixture = league.GetFixtures().First();

            // No bookmakers chosen in settings by default
            Assert.AreEqual(fixture.Odds.Count, 5);

        }
    }
}