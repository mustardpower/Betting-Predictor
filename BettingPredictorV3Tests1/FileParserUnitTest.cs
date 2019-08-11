using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BettingPredictorV3;
using BettingPredictorV3.DataStructures;
using System.Linq;

namespace BettingPredictorV3Tests
{
    [TestClass]
    public class FileParserUnitTest
    {
        [TestMethod]
        public void TestParseUpcomingFixtures11thAugust2019()
        {
            string testHtmlCode = @"Div,Date,Time,HomeTeam,AwayTeam,FTHG,FTAG,FTR,HTHG,HTAG,HTR,B365H,B365D,B365A,BWH,BWD,BWA,IWH,IWD,IWA,PSH,PSD,PSA,WHH,WHD,WHA,VCH,VCD,VCA,MaxH,MaxD,MaxA,AvgH,AvgD,AvgA,B365>2.5,B365<2.5,P>2.5,P<2.5,Max>2.5,Max<2.5,Avg>2.5,Avg<2.5,AHh,B365AHH,B365AHA,PAHH,PAHA,MaxAHH,MaxAHA,AvgAHH,AvgAHA,B365CH,B365CD,B365CA,BWCH,BWCD,BWCA,IWCH,IWCD,IWCA,PSCH,PSCD,PSCA,WHCH,WHCD,WHCA,VCCH,VCCD,VCCA,MaxCH,MaxCD,MaxCA,AvgCH,AvgCD,AvgCA,B365C>2.5,B365C<2.5,PC>2.5,PC<2.5,MaxC>2.5,MaxC<2.5,AvgC>2.5,AvgC<2.5,AHCh,B365CAHH,B365CAHA,PCAHH,PCAHA,MaxCAHH,MaxCAHA,AvgCAHH,AvgCAHA
B1,09 / 08 / 2019,19:30,Anderlecht,Mechelen,,,,,,,1.75,4.2,4,1.83,3.75,3.9,1.77,3.8,3.95,1.82,3.88,4.36,1.75,3.8,4.2,1.75,4,4.2,1.86,4.2,4.36,1.79,3.81,4.07,1.6,2.3,1.58,2.43,1.62,2.43,1.58,2.31,-0.75,1.98,1.88,2.04,1.84,2.04,1.94,2,1.84,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,";

            FileParser fileParser = new FileParser();
            fileParser.ParseUpcomingFixtures(testHtmlCode);
            Assert.AreEqual(fileParser.Database.FixtureList.Count, 1);

            Fixture fixture = fileParser.Database.FixtureList.First();

            Assert.AreEqual(fixture.LeagueID, "B1");
            Assert.AreEqual(fixture.Date.Day, 9);
            Assert.AreEqual(fixture.Date.Month, 8);
            Assert.AreEqual(fixture.Date.Year, 2019);

            Assert.AreEqual(fixture.HomeTeam.Name, "Anderlecht");
            Assert.AreEqual(fixture.AwayTeam.Name, "Mechelen");
        }
    }
}
