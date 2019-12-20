using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BettingPredictorV3;
using BettingPredictorV3.DataStructures;
using System.Collections.Generic;
using System.Linq;

namespace BettingPredictorV3Tests
{
    [TestClass]
    public class ResultPredictorUnitTest
    {
        [TestMethod]
        public void TestTryPredictFixtureWithNoData()
        {
            League league = new League("TEST");
            Team homeTeam = new Team("HOME");
            Team awayTeam = new Team("AWAY");
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

        [TestMethod]
        public void TestWeightingFunctionChangesValues()
        {
            ResultPredictor resultPredictor = new ResultPredictor();
            List<double> sample = new List<double>()
            {
                1, 2, 3, 4, 5, 6, 7, 8
            };

            List<double> weightedSample = resultPredictor.WeightingFunction(sample);

            // test collections are the different by checking there
            //are items in one list but not the other
            Assert.IsFalse(weightedSample.Except(sample).Count() == 0);
            Assert.IsFalse(sample.Except(weightedSample).Count() == 0);
        }

    }
}
