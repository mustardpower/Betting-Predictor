using System;
using BettingPredictorV3.DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BettingPredictorV3.DataStructures.Tests
{
    [TestClass]
    public class FixtureUnitTest
    {
        [TestMethod]
        public void TestHasCalculatedOdds()
        {
            Fixture testFixture = new Fixture();
            Assert.IsFalse(testFixture.HasCalculatedOdds());

            testFixture.BestHomeOdds = new Bookmaker("Test Bookmaker", 2, 3, 1);
            Assert.IsFalse(testFixture.HasCalculatedOdds());

            testFixture.BestDrawOdds = new Bookmaker("Test Bookmaker 2", 1.9, 3.2, 1);
            Assert.IsFalse(testFixture.HasCalculatedOdds());

            testFixture.BestAwayOdds = new Bookmaker("Test Bookmaker 3", 1.9, 2.9, 1.4);
            Assert.IsTrue(testFixture.HasCalculatedOdds());
        }
    }
}
