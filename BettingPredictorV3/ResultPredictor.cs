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

            var previousHomeFixtures = GetFixturesBefore(fixture.HomeTeam, fixture.Date);
            var previousAwayFixtures = GetFixturesBefore(fixture.AwayTeam, fixture.Date);
            fixture.CalculateGoalsPerGame(previousHomeFixtures, previousAwayFixtures);

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
                double leagueAverageHomeGoals = fixture.FixtureLeague.GetAverageHomeGoals(fixture.Date);
                fixture.PredictedHomeGoals = CalculatePredictedHomeGoals(homeSample, awayOppSample, leagueAverageHomeGoals, alpha);
                double leagueAverageAwayGoals = fixture.FixtureLeague.GetAverageAwayGoals(fixture.Date);
                fixture.PredictedAwayGoals = CalculatePredictedAwayGoals(awaySample, homeOppSample, leagueAverageAwayGoals, beta);

                fixture.PredictedGoalDifference = fixture.PredictedHomeGoals - fixture.PredictedAwayGoals;

                fixture.CalculateResiduals();

                List<double> homeResiduals = GetResiduals(DateTime.Now, fixture.HomeTeam);
                fixture.AverageHomeResidual = homeResiduals.Count > 0 ? homeResiduals.Average() : 0.0;

                List<double> awayResiduals = GetResiduals(DateTime.Now, fixture.AwayTeam);
                fixture.AverageAwayResidual = awayResiduals.Count > 0 ? awayResiduals.Average() : 0.0;

                fixture.BothToScore = CalculateBothToScore(fixture.PredictedHomeGoals, fixture.PredictedAwayGoals);

                fixture.Odds = dbContext.Bookmakers.Where(x => x.FixtureId == fixture.FixtureId).ToList();
                fixture.FindBestOdds();
                fixture.CalculateKellyCriterion();

                dbContext.SaveChanges();
            }
        }

        public List<Fixture> GetFixturesBefore(Team aTeam, DateTime date)
        {
            var homeFixtures = dbContext.Fixtures.Where(fixture => fixture.Date < date && fixture.HomeTeamId == aTeam.TeamId);
            var awayFixtures = dbContext.Fixtures.Where(fixture => fixture.Date < date && fixture.AwayTeamId == aTeam.TeamId);
            return homeFixtures.Concat(awayFixtures).ToList();
        }

        private double CalculateBothToScore(double predictedHomeGoals, double predictedAwayGoals)
        {
            // subtract probabilities from 1.0 for the following results: 0-0, 1-0, 2-0, 3- 0 ....., 0-1, 0-2, 0-3....
            double bothToScore = 1.0;

            double home_prob_no_goals = StatsLib.poissonPDF(predictedHomeGoals, 0);
            double away_prob_no_goals = StatsLib.poissonPDF(predictedAwayGoals, 0);

            bothToScore -= (home_prob_no_goals * away_prob_no_goals);  // P(A = 0 & B = 0) 
            bothToScore -= ((1 - home_prob_no_goals) * away_prob_no_goals); // P(A != 0 & B = 0) 
            bothToScore -= (home_prob_no_goals * (1 - away_prob_no_goals)); // P(A = 0 & B != 0) 
            return bothToScore;
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

        public double CalculatePredictedHomeGoals(List<double> home_sample, List<double> away_opp_sample, double leagueAverageHomeGoals, double alpha)
        {
            double home_attack_strength;
            home_attack_strength = home_sample.Average() / leagueAverageHomeGoals;

            double away_defence_strength;
            away_defence_strength = away_opp_sample.Average() / leagueAverageHomeGoals;

            return home_attack_strength * away_defence_strength * leagueAverageHomeGoals - alpha;
        }

        public double CalculatePredictedAwayGoals(List<double> away_sample, List<double> home_opp_sample, double leagueAverageAwayGoals, double beta)
        {
            double away_attack_strength;
            away_attack_strength = away_sample.Average() / leagueAverageAwayGoals;

            double home_defence_strength;
            home_defence_strength = home_opp_sample.Average() / leagueAverageAwayGoals;

            return away_attack_strength * home_defence_strength * leagueAverageAwayGoals - beta;
        }

        private List<Fixture> FixturesForTeam(Team aTeam)
        {
            Team teamInDatabase = dbContext.Teams.Where(team => team.TeamId == aTeam.TeamId).FirstOrDefault();
            if (teamInDatabase == null) return new List<Fixture>();

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
