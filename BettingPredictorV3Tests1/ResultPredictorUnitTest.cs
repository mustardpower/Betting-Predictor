using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BettingPredictorV3;
using BettingPredictorV3.DataStructures;
using System.Collections.Generic;

namespace BettingPredictorV3Tests
{
    [TestClass]
    public class ResultPredictorUnitTest
    {
        [TestMethod]
        public void TestTryPredictFixtureWithNoData()
        {
            League league = new League("TEST");
            Team homeTeam = new Team(league, "HOME");
            Team awayTeam = new Team(league, "AWAY");
            ResultPredictor resultPredictor = new ResultPredictor();
            DateTime date = new DateTime();
            Referee referee = new Referee("Ref");
            List<Bookmaker> odds = new List<Bookmaker>();
            Fixture fixture = new Fixture(league, date, homeTeam, awayTeam, referee, odds);

            Assert.AreEqual(0, fixture.PredictedHomeGoals);
            Assert.AreEqual(0, fixture.PredictedAwayGoals);
            resultPredictor.PredictResult(fixture, 0, 0);
            Assert.AreEqual(0, fixture.PredictedHomeGoals, "Should not have changed since there are no previous fixtures");
            Assert.AreEqual(0, fixture.PredictedAwayGoals, "Should not have changed since there are no previous fixtures");
        }
    }
}
