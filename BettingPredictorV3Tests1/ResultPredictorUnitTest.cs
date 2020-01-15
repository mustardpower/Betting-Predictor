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

        [TestMethod]
        public void TestWeightingFunctionChangesValues()
        {
            List<double> sample = new List<double>()
            {
                1, 2, 3, 4, 5, 6, 7, 8
            };

            List<double> weightedSample = ResultPredictor.WeightingFunction(sample);

            // test collections are the different by checking there
            //are items in one list but not the other
            Assert.IsFalse(weightedSample.Except(sample).Count() == 0);
            Assert.IsFalse(sample.Except(weightedSample).Count() == 0);
        }

        [TestMethod]
        public void TestGoodnessOfFitPoisson()
        {
            // these are values of PDF where lambda = 2
            List<double> sample = new List<double>()
            {
                0, 0, 0, 0, 0, // team scored 0 in 5 games 
                1, 1, 1, 1, 1, 1, 1, // team scored 1 in 7 games 
                2, 2, 2, 2, 2, 2, 2, 2, // team scored 2 in 8 games
                3, 3, // team scored 3 in 2 games
                4 // team scored 4 in 1 game
            };

            double lambda = sample.Average();
            Assert.AreEqual(lambda, 1.43478, 0.001, "Unexpected average goals in sample");

            Assert.AreEqual(StatsLib.poissonPDF(lambda, 0), 0.23817, 0.0001, "Unexpected frequency for 0 goals scored");
            Assert.AreEqual(StatsLib.poissonPDF(lambda, 1), 0.34172, 0.0001, "Unexpected frequency for 1 goals scored");
            Assert.AreEqual(StatsLib.poissonPDF(lambda, 2), 0.24515, 0.0001, "Unexpected frequency for 2 goals scored");
            Assert.AreEqual(StatsLib.poissonPDF(lambda, 3), 0.11724, 0.0001, "Unexpected frequency for 3 goals scored");
            Assert.AreEqual(StatsLib.poissonPDF(lambda, 4), 0.04205, 0.0001, "Unexpected frequency for 4 goals scored");
            Assert.AreEqual(StatsLib.poissonPDF(lambda, 5), 0.01207, 0.0001, "Unexpected frequency for 5 goals scored");

            Assert.AreEqual(5.47784, ResultPredictor.expectedFrequencyPoisson(lambda, 0, sample.Count), 0.0001);
            Assert.AreEqual(7.85952, ResultPredictor.expectedFrequencyPoisson(lambda, 1, sample.Count), 0.0001);
            Assert.AreEqual(5.63835, ResultPredictor.expectedFrequencyPoisson(lambda, 2, sample.Count), 0.0001);
            Assert.AreEqual(2.69660, ResultPredictor.expectedFrequencyPoisson(lambda, 3, sample.Count), 0.0001);
            Assert.AreEqual(0.96726, ResultPredictor.expectedFrequencyPoisson(lambda, 4, sample.Count), 0.0001);
            Assert.AreEqual(0.27756, ResultPredictor.expectedFrequencyPoisson(lambda, 5, sample.Count), 0.0001);

            List<double> expectedFrequencySample = new List<double>()
            {
                5.47784,7.85952,5.63835,2.69660,0.96726, 0.27756
            };

            List<double> actualFrequencySample = new List<double>()
            { 5, 7, 8, 2, 1, 0};

            double chiSquaredValue = StatsLib.ChiSquaredValue(actualFrequencySample, expectedFrequencySample);

            Assert.AreEqual(1.58349, chiSquaredValue, 0.0001, "Unexpected chi-squared statistic calculated");

        }

    }
}
