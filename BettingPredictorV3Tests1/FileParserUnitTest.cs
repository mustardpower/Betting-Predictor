﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BettingPredictorV3;
using BettingPredictorV3.DataStructures;
using System.Linq;
using System.Collections.Generic;

namespace BettingPredictorV3Tests
{
    [TestClass]
    public class FileParserUnitTest
    {
        [TestMethod]
        public void TestParseUpcomingFixtures11thAugust2019()
        {
            DatabaseSettings.BookmakersUsed = DatabaseSettings.DefaultBookmakers();

            string testHtmlCode = @"Div,Date,Time,HomeTeam,AwayTeam,FTHG,FTAG,FTR,HTHG,HTAG,HTR,B365H,B365D,B365A,BWH,BWD,BWA,IWH,IWD,IWA,PSH,PSD,PSA,WHH,WHD,WHA,VCH,VCD,VCA,MaxH,MaxD,MaxA,AvgH,AvgD,AvgA,B365>2.5,B365<2.5,P>2.5,P<2.5,Max>2.5,Max<2.5,Avg>2.5,Avg<2.5,AHh,B365AHH,B365AHA,PAHH,PAHA,MaxAHH,MaxAHA,AvgAHH,AvgAHA,B365CH,B365CD,B365CA,BWCH,BWCD,BWCA,IWCH,IWCD,IWCA,PSCH,PSCD,PSCA,WHCH,WHCD,WHCA,VCCH,VCCD,VCCA,MaxCH,MaxCD,MaxCA,AvgCH,AvgCD,AvgCA,B365C>2.5,B365C<2.5,PC>2.5,PC<2.5,MaxC>2.5,MaxC<2.5,AvgC>2.5,AvgC<2.5,AHCh,B365CAHH,B365CAHA,PCAHH,PCAHA,MaxCAHH,MaxCAHA,AvgCAHH,AvgCAHA
B1,09 / 08 / 2019,19:30,Anderlecht,Mechelen,,,,,,,1.75,4.2,4,1.83,3.75,3.9,1.77,3.8,3.95,1.82,3.88,4.36,1.75,3.8,4.2,1.75,4,4.2,1.86,4.2,4.36,1.79,3.81,4.07,1.6,2.3,1.58,2.43,1.62,2.43,1.58,2.31,-0.75,1.98,1.88,2.04,1.84,2.04,1.94,2,1.84,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,";

            FileParser fileParser = new FileParser();
            List<FixtureDTO> fixtures = fileParser.ParseUpcomingFixtures(testHtmlCode);
            Assert.AreEqual(fixtures.Count, 1);

            FixtureDTO fixture = fixtures.First();

            Assert.AreEqual("B1", fixture.LeagueCode);
            Assert.AreEqual(9, fixture.Date.Day);
            Assert.AreEqual(8, fixture.Date.Month);
            Assert.AreEqual(2019, fixture.Date.Year);

            Assert.AreEqual("Anderlecht", fixture.HomeTeamName);
            Assert.AreEqual("Mechelen", fixture.AwayTeamName);

            Assert.AreEqual("Bet 365", fixture.Odds[0].Name);
            Assert.AreEqual(1.75, fixture.Odds[0].HomeOdds);
            Assert.AreEqual(4.2, fixture.Odds[0].DrawOdds);
            Assert.AreEqual(4, fixture.Odds[0].AwayOdds);

            Assert.AreEqual("BetWin", fixture.Odds[1].Name);
            Assert.AreEqual(1.83, fixture.Odds[1].HomeOdds);
            Assert.AreEqual(3.75, fixture.Odds[1].DrawOdds);
            Assert.AreEqual(3.9, fixture.Odds[1].AwayOdds);

            Assert.AreEqual("InterWetten", fixture.Odds[2].Name);
            Assert.AreEqual(1.77, fixture.Odds[2].HomeOdds);
            Assert.AreEqual(3.8, fixture.Odds[2].DrawOdds);
            Assert.AreEqual(3.95, fixture.Odds[2].AwayOdds);

            Assert.AreEqual("Pinnacle Sport", fixture.Odds[3].Name);
            Assert.AreEqual(1.82, fixture.Odds[3].HomeOdds);
            Assert.AreEqual(3.88, fixture.Odds[3].DrawOdds);
            Assert.AreEqual(4.36, fixture.Odds[3].AwayOdds);

            Assert.AreEqual("William Hill", fixture.Odds[4].Name);
            Assert.AreEqual(1.75, fixture.Odds[4].HomeOdds);
            Assert.AreEqual(3.8, fixture.Odds[4].DrawOdds);
            Assert.AreEqual(4.2, fixture.Odds[4].AwayOdds);

            Assert.AreEqual("Victor Chandler", fixture.Odds[5].Name);
            Assert.AreEqual(1.75, fixture.Odds[5].HomeOdds);
            Assert.AreEqual(4, fixture.Odds[5].DrawOdds);
            Assert.AreEqual(4.2, fixture.Odds[5].AwayOdds);
        }

        [TestMethod]
        public void TestHistoricalOdds()
        {
            Database database = new Database();

            FileParser parser = new FileParser();

            List<string> bookmakersSelected = new List<string>();
            bookmakersSelected.Add("William Hill");

            DatabaseSettings.BookmakersUsed = bookmakersSelected;

            List<HistoricalFixtureDTO> historicalFixtures = parser.LoadHistoricalFile("..\\..\\Test Files\\I1 (22-12-19).csv");
            database.AddFixtures(historicalFixtures.ToList<IDatabaseObject<Fixture>>());

            Team homeTeam = database.GetTeam("I1", "Brescia");
            Fixture testFixture = homeTeam.Fixtures.Find(fixture => fixture.Date == new DateTime(2019, 12, 14));
            Assert.AreEqual("Brescia", testFixture.HomeTeam.Name);
            Assert.AreEqual("Lecce", testFixture.AwayTeam.Name);

            Assert.AreEqual("William Hill", testFixture.BestHomeOdds.Name);
            Assert.AreEqual(2, testFixture.BestHomeOdds.HomeOdds);

            Assert.AreEqual("William Hill", testFixture.BestDrawOdds.Name);
            Assert.AreEqual(3.7, testFixture.BestDrawOdds.DrawOdds);

            Assert.AreEqual("William Hill", testFixture.BestAwayOdds.Name);
            Assert.AreEqual(3.5, testFixture.BestAwayOdds.AwayOdds);

            DatabaseSettings.BookmakersUsed = DatabaseSettings.DefaultBookmakers();
        }
    }
}
