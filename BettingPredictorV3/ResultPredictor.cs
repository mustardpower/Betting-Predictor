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
        private FootballResultsDbContext dbContext;

        public ResultPredictor(FootballResultsDbContext aContext)
        {
            dbContext = aContext;
        }

        public void PredictResult(Fixture fixture, double alpha, double beta)
        {
            List<double> homeSample;
            List<double> awaySample;
            List<double> homeOppSample;
            List<double> awayOppSample;

            fixture.CalculateGoalsPerGame();
            fixture.HomeForm = CalculateForm(fixture.Date, fixture.HomeTeam);
            fixture.AwayForm = CalculateForm(fixture.Date, fixture.AwayTeam);

            homeSample = CreateHomeSample(fixture.Date, fixture.HomeTeam);   // create the samples
            awaySample = CreateAwaySample(fixture.Date, fixture.AwayTeam);

            if ((homeSample.Count != 0) && (awaySample.Count != 0))
            {
                homeOppSample = CreateHomeOppositionSample(fixture.Date, fixture.HomeTeam);
                awayOppSample = CreateAwayOppositionSample(fixture.Date, fixture.AwayTeam);

                homeSample = WeightingFunction(homeSample);
                awaySample = WeightingFunction(awaySample);

                // calculates a home attacking strength and defence strength
                fixture.CalculateStrengths(homeSample, awaySample, homeOppSample, awayOppSample, alpha, beta);

                fixture.PredictedGoalDifference = fixture.PredictedHomeGoals - fixture.PredictedAwayGoals;

                fixture.CalculateResiduals();

                List<double> homeResiduals = GetResiduals(DateTime.Now, fixture.HomeTeam);
                fixture.AverageHomeResidual = homeResiduals.Count > 0 ? homeResiduals.Average() : 0.0;

                List<double> awayResiduals = GetResiduals(DateTime.Now, fixture.AwayTeam);
                fixture.AverageAwayResidual = awayResiduals.Count > 0 ? awayResiduals.Average() : 0.0;

                fixture.CalculateBothToScore();
                fixture.CalculateKellyCriterion();
            }
        }

        private List<double> CreateHomeSample(DateTime date, Team aTeam)
        {
            List<double> sample = new List<double>();
            var fixtures = FixturesForTeam(aTeam);
            foreach (Fixture fixture in fixtures)
            {
                if (fixture.Date < date)
                {
                    if (fixture.HomeTeam == aTeam)
                    {
                        sample.Add(fixture.HomeGoals);
                    }
                }
            }

            return sample;
        }

        private List<double> CreateHomeOppositionSample(DateTime date, Team aTeam)
        {
            List<double> sample = new List<double>();
            var fixtures = FixturesForTeam(aTeam);
            foreach (Fixture fixture in fixtures)
            {
                if (fixture.Date < date)
                {
                    if (fixture.HomeTeam == aTeam)
                    {
                        sample.Add(fixture.AwayGoals);
                    }
                }
            }

            return sample;
        }

        private List<double> CreateAwayOppositionSample(DateTime date, Team aTeam)
        {
            List<double> sample = new List<double>();
            var fixtures = FixturesForTeam(aTeam);
            foreach (Fixture fixture in fixtures)
            {
                if (fixture.Date < date)
                {
                    if (fixture.AwayTeam == aTeam)
                    {
                        sample.Add(fixture.HomeGoals);
                    }
                }
            }

            return sample;
        }

        private List<Fixture> FixturesForTeam(Team aTeam)
        {
            Team teamInDatabase = dbContext.Teams.Where(team => aTeam.TeamId == aTeam.TeamId).FirstOrDefault();
            var combinedFixtures = teamInDatabase.HomeFixtures.Concat(teamInDatabase.AwayFixtures).ToList();
            return combinedFixtures;
        }

        private List<double> CreateAwaySample(DateTime date, Team aTeam)
        {
            List<double> sample = new List<double>();
            var fixtures = FixturesForTeam(aTeam);
            foreach (Fixture fixture in fixtures)
            {
                if (fixture.Date < date)
                {
                    if (fixture.AwayTeam == aTeam)
                    {
                        sample.Add(fixture.AwayGoals);
                    }
                }
            }

            return sample;
        }

        public int CalculateForm(DateTime date, Team aTeam)
        {
            int idx = 0;
            int form = 0;
            const int kNumberOfRelevantGames = 5;
            const int kNumberOfPtsForWin = 3;

            var homeFixtures = dbContext.Fixtures.Where(fixture => fixture.Date < date && fixture.HomeTeamId == aTeam.TeamId);
            var awayFixtures = dbContext.Fixtures.Where(fixture => fixture.Date < date && fixture.AwayTeamId == aTeam.TeamId);
            List<Fixture> previous_results = homeFixtures.Concat(awayFixtures).ToList();
            previous_results.Reverse();

            foreach (Fixture fixture in previous_results)
            {
                if (idx < kNumberOfRelevantGames)
                {
                    if (fixture.HomeTeam == aTeam) // if current team is home side
                    {
                        if (fixture.AwayGoals < fixture.HomeGoals)	// home win
                        {
                            form += kNumberOfPtsForWin;
                        }
                        else if (fixture.AwayGoals == fixture.HomeGoals) // draw
                        {
                            form++;
                        }
                    }
                    else // if current team is the away side
                    {
                        if (fixture.AwayGoals > fixture.HomeGoals)	// away win
                        {
                            form += kNumberOfPtsForWin;
                        }
                        else if (fixture.AwayGoals == fixture.HomeGoals) // draw
                        {
                            form++;
                        }
                    }

                    idx++;
                }
            }

            return form;
        }

        public List<double> WeightingFunction(List<double> sample)
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

        private List<double> GetResiduals(DateTime date, Team aTeam)
        {
            List<double> residuals = new List<double>();
            double residual;
            var fixtures = FixturesForTeam(aTeam);

            foreach (Fixture fixture in fixtures)
            {
                if (fixture.Date < date)
                {
                    if (fixture.HomeTeam == aTeam)
                    {
                        residual = fixture.HomeResidual;
                        if (!Double.IsNaN(residual))
                        {
                            residuals.Add(residual);
                        }
                    }
                    else if (fixture.AwayTeam == aTeam)
                    {
                        residual = fixture.AwayResidual;
                        if (!Double.IsNaN(residual))
                        {
                            residuals.Add(residual);
                        }
                    }
                }
            }

            return residuals;
        }
    }
}
