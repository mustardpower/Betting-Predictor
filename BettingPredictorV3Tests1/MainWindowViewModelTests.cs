using Microsoft.VisualStudio.TestTools.UnitTesting;
using BettingPredictorV3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BettingPredictorV3.DataStructures;

namespace BettingPredictorV3.Tests
{
    [TestClass()]
    public class MainWindowViewModelTests
    {
        [TestMethod()]
        public void UpcomingFixtureDatesTest()
        {
            League testLeague = new League("TEST");
            Team teamA = new Team(testLeague, "Team A");
            Team teamB = new Team(testLeague, "Team B");

            List<Fixture> fixtureList = new List<Fixture>()
            {
                new Fixture(testLeague, new DateTime(2019, 12, 30), teamA, teamB, new Referee("Test Referee"), new List<Bookmaker>()),
                new Fixture(testLeague, new DateTime(2020, 1, 1, 12, 30, 0), teamB, teamA, new Referee("Test Referee"), new List<Bookmaker>()),
                new Fixture(testLeague, new DateTime(2020, 1, 1, 15, 0, 0), teamB, teamA, new Referee("Test Referee"), new List<Bookmaker>()),
            };

            var dates = MainWindowViewModel.DatesForFixtures(fixtureList);
            Assert.AreEqual(2, dates.Count());

            Assert.AreEqual(2019, dates.First().Year);
            Assert.AreEqual(12, dates.First().Month);
            Assert.AreEqual(30, dates.First().Day);

            Assert.AreEqual(2020, dates.ElementAt(1).Year);
            Assert.AreEqual(1, dates.ElementAt(1).Month);
            Assert.AreEqual(1, dates.ElementAt(1).Day);
        }
    }
}