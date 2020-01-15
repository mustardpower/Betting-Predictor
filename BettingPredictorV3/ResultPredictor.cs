using BettingPredictorV3.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingPredictorV3
{
    public class ResultPredictor
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

                homeSample = WeightingFunction(homeSample);
                awaySample = WeightingFunction(awaySample);

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

        public static List<double> WeightingFunction(List<double> sample)
        {
            List<double> new_sample = new List<double>();
            int n = sample.Count;
            double idx = 1;
            double k; // weighting variable
            double log_x;

            double sum = 0;

            foreach (double x in sample)
            {
                if (n > 2)
                {
                    sum += Math.Log(idx) / Math.Log((double)(n / 2));	// tally up values of log^x to the base n/2
                }
                idx++;
            }

            if (sum == 0)
            {
                k = 0;
            }
            else
            {
                k = n / sum;
            }
            idx = 1;

            foreach (double x in sample)
            {
                if (n > 2)
                {
                    log_x = Math.Log(idx) / Math.Log((double)(n / 2));
                }
                else
                {
                    log_x = 0;
                }
                double new_x = k * log_x * (x);
                idx++;

                new_sample.Add(new_x);
            }

            return new_sample;
        }

        public static double expectedFrequencyPoisson(double lambda, int k, int v)
        {
            return StatsLib.poissonPDF(lambda, k) * v;
        }

        public static double GoodnessOfFitPoisson(List<double> sample)
        {
            return 0.0;
        }
    }
}
