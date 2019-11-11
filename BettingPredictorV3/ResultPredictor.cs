using BettingPredictorV3.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingPredictorV3
{
    class ResultPredictor
    {
        public void PredictResult(Fixture fixture, double alpha, double beta)
        {
            List<double> homeSample;
            List<double> awaySample;
            List<double> homeOppSample;
            List<double> awayOppSample;

            fixture.CalculateGoalsPerGame();
            fixture.HomeForm = fixture.HomeTeam.CalculateForm(fixture.Date);
            fixture.AwayForm = fixture.AwayTeam.CalculateForm(fixture.Date);

            homeSample = fixture.HomeTeam.CreateHomeSample(fixture.Date);   // create the samples
            awaySample = fixture.AwayTeam.CreateAwaySample(fixture.Date);

            if ((homeSample.Count != 0) && (awaySample.Count != 0))
            {
                homeOppSample = fixture.HomeTeam.CreateHomeOppositionSample(fixture.Date);
                awayOppSample = fixture.AwayTeam.CreateAwayOppositionSample(fixture.Date);

                homeSample = fixture.WeightingFunction(homeSample);
                awaySample = fixture.WeightingFunction(awaySample);

                // calculates a home attacking strength and defence strength
                fixture.CalculateStrengths(homeSample, awaySample, homeOppSample, awayOppSample, alpha, beta);

                fixture.PredictedGoalDifference = fixture.PredictedHomeGoals - fixture.PredictedAwayGoals;

                fixture.CalculateResiduals();

                List<double> homeResiduals = fixture.HomeTeam.GetResiduals(DateTime.Now);
                fixture.AverageHomeResidual = homeResiduals.Count > 0 ? homeResiduals.Average() : 0.0;

                List<double> awayResiduals = fixture.AwayTeam.GetResiduals(DateTime.Now);
                fixture.AverageAwayResidual = awayResiduals.Count > 0 ? awayResiduals.Average() : 0.0;

                fixture.CalculateBothToScore();
                fixture.CalculateKellyCriterion();
            }
        }
    }
}
