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

            List<string> columnHeaders = new List<string>()
            {
                "Div", "Date",    "HomeTeam",    "AwayTeam", 
                "FTHG",    "FTAG" ,   "FTR", "HTHG",    "HTAG",    "HTR",
                "B365H",   "B365D",   "B365A",
                "BWH", "BWD", "BWA",
                "IWH", "IWD", "IWA",
                "LBH", "LBD", "LBA",
                "PSH", "PSD", "PSA",
                "WHH", "WHD", "WHA",
                "SJH", "SJD" ,"SJA",
                "VCH", "VCD", "VCA",
                "Bb1X2",   "BbMxH",   "BbAvH",   "BbMxD",   "BbAvD",   "BbMxA", 
                "BbAvA",   "BbOU",    "BbMx>2.5",    "BbAv>2.5",    "BbMx<2.5",    "BbAv<2.5", 
                "BbAH",    "BbAHh",   "BbMxAHH", "BbAvAHH", "BbMxAHA", "BbAvAHA", "PSCH","PSCD","PSCA"

            };

            var fixture = FileParser.ParseHistoricalData(testData, columnHeaders);

            Assert.AreEqual(league.LeagueID, "SP2");

            Assert.AreEqual(fixture.LeagueCode, "SP2");
            Assert.AreEqual(fixture.Date.Day, 16);
            Assert.AreEqual(fixture.Date.Month, 8);
            Assert.AreEqual(fixture.Date.Year, 2013);

            Assert.AreEqual(fixture.HomeTeamName, "Girona");
            Assert.AreEqual(fixture.AwayTeamName, "Alaves");
            Assert.AreEqual(fixture.HomeGoals, 1);
            Assert.AreEqual(fixture.AwayGoals, 0);
        }

        [TestMethod()]
        public void ParseBookmakerOddsTest()
        {
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

            List<string> columnHeaders = new List<string>()
            {
                "Div", "Date",    "HomeTeam",    "AwayTeam",
                "FTHG",    "FTAG" ,   "FTR", "HTHG",    "HTAG",    "HTR",
                "B365H",   "B365D",   "B365A",
                "BWH", "BWD", "BWA",
                "IWH", "IWD", "IWA",
                "LBH", "LBD", "LBA",
                "PSH", "PSD", "PSA",
                "WHH", "WHD", "WHA",
                "SJH", "SJD" ,"SJA",
                "VCH", "VCD", "VCA",
                "Bb1X2",   "BbMxH",   "BbAvH",   "BbMxD",   "BbAvD",   "BbMxA",
                "BbAvA",   "BbOU",    "BbMx>2.5",    "BbAv>2.5",    "BbMx<2.5",    "BbAv<2.5",
                "BbAH",    "BbAHh",   "BbMxAHH", "BbAvAHH", "BbMxAHA", "BbAvAHA", "PSCH","PSCD","PSCA"

            };

            var historicalFixture = FileParser.ParseHistoricalData(testData, columnHeaders);

            // No bookmakers chosen in settings by default
            Assert.AreEqual(historicalFixture.Odds.Count, 5);

        }
    }
}