using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BettingPredictorV3.DataStructures
{
    [Serializable]
    public class Fixture
    {
        private League league;
        private readonly DateTime date;
        private Team homeTeam;
        private Team awayTeam;
        private readonly double homeGoals;
        private readonly double awayGoals;
        private readonly Referee referee;
        private float homeGoalsPerGame;
        private float awayGoalsPerGame;
        private double predictedHomeGoals;
        private double predictedAwayGoals;
        private double predictedGoalDifference;
        private double homeResidual;
        private double awayResidual;
        private double averageHomeResidual;
        private double averageAwayResidual;
        private double bothToScore;   // probability that both teams score in the fixture
        private double? homeWinProbabilty;
        private double? drawProbability;
        private double? awayWinProbability;

        public int HomeForm { get; set; }
        public int AwayForm { get; set; }

        public List<Bookmaker> odds;

        public Fixture()
        {
        }

        public Fixture(League league, DateTime date, Team home_team, Team away_team, Referee referee,List<Bookmaker> odds) // constructor for fixture
        {
            this.league = league;
            this.date = date;
            this.homeTeam = home_team;
            this.awayTeam = away_team;
            this.referee = referee;
            this.odds = odds;
            predictedHomeGoals = 0;
            predictedAwayGoals = 0;
            bothToScore = 0.0;

            FindBestOdds();
        }
        public Fixture(League league,DateTime date,Team home_team,Team away_team,double home_goals,double away_goals,Referee referee,List<Bookmaker> odds) // for result
        {
            this.league = league;
            this.date = date;
            this.homeTeam = home_team;
            this.awayTeam = away_team;
            this.homeGoals = home_goals;
            this.awayGoals = away_goals;
            this.referee = referee;
            this.odds = odds;
            predictedHomeGoals = 0;
            predictedAwayGoals = 0;

            FindBestOdds();
        }

        public League League
        {
            get
            {
                return league;
            }
        }

        public Team HomeTeam
        {
            get
            {
                return homeTeam;
            }
        }

        public Team AwayTeam
        {
            get
            {
                return awayTeam;
            }
        }

        public double HomeGoals
        {
            get
            {
                return homeGoals;
            }
        }

        public double AwayGoals
        {
            get
            {
                return awayGoals;
            }
        }

        public double HomeResidual
        {
            get
            {
                return homeResidual;
            }
        }

        public double AwayResidual
        {
            get
            {
                return awayResidual;
            }
        }

        public DateTime Date
        {
            get
            {
                return date;
            }
        }


        public String LeagueID
        {
            get
            {
                return league.LeagueID;
            } 
        }

        public Bookmaker BestHomeOdds { get; set; }

        public Bookmaker BestDrawOdds { get; set; }

        public Bookmaker BestAwayOdds { get; set; }

        public double PredictedHomeGoals
        {
            get
            {
                return predictedHomeGoals;
            }
        }

        public double PredictedAwayGoals
        {
            get
            {
                return predictedAwayGoals;
            }
        }

        public double BothToScore
        {
            get
            {
                return bothToScore;
            }
        }


        public double PredictedGoalDifference { get => predictedGoalDifference; set => predictedGoalDifference = value; }
        public double Home_residual { get => homeResidual; set => homeResidual = value; }
        public double Away_residual { get => awayResidual; set => awayResidual = value; }
        public double AverageHomeResidual { get => averageHomeResidual; set => averageHomeResidual = value; }
        public double AverageAwayResidual { get => averageAwayResidual; set => averageAwayResidual = value; }
        public double Arbitrage { get; private set; }
        public double KellyCriterionHome { get; private set; }
        public double KellyCriterionDraw { get; private set; }
        public double KellyCriterionAway { get; private set; }

        public double HomeWinProbability {
            get
            {
                if (homeWinProbabilty == null)
                {
                    homeWinProbabilty = CalculateHomeWinProbability();
                }

                return (double)homeWinProbabilty;
            }
        }

        public double DrawProbability
        {
            get
            {
                if (drawProbability == null)
                {
                    drawProbability = CalculateDrawProbability();
                }

                return (double)drawProbability;
            }
        }

        public double AwayWinProbability
        {
            get
            {
                if (awayWinProbability == null)
                {
                    awayWinProbability = CalculateAwayWinProbability();
                }

                return (double)awayWinProbability;
            }
        }


        public List<double> HomeWeightingFunction(List<double> sample)
        {
            List<double> new_sample = new List<double>();
            int n = sample.Count;
	        double idx = 1;
	        double k; // weighting variable
	        double log_x;

	        double sum = 0;

	        foreach(double x in sample)
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

	        foreach(double x in sample)
	        {
                if (n > 2)
                {
                    log_x = Math.Log(idx) / Math.Log((double)(n / 2));
                }
                else
                {
                    log_x = 0;
                }
		        double new_x = k*log_x*(x);
		        idx++;

                new_sample.Add(new_x);
	        }

            return new_sample;
        }
        public List<double> AwayWeightingFunction(List<double> sample)
        {
            List<double> new_sample = new List<double>();
            int n = sample.Count;
            double m = 0;	// the total number of goals scored by the team
            double idx = 1;
            double k; // weighting variable
            double log_x;

            double sum = 0;

            foreach (double x in sample)
            {
                m += x;		// sum all the goals in the sample

                if (n > 2)
                {
                    sum += Math.Log(idx) / Math.Log((double)(n / 2));	// tally up values of log^x to the base n/2
                }
                idx++;
            }

            m = m / n;	// m is now the mean goals of the sample

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

        public void CalculateResiduals()
        {
            homeResidual = homeGoals - predictedHomeGoals;
            awayResidual = awayGoals - predictedAwayGoals;
        }

        public void CalculateGoalsPerGame()
        {
            homeGoalsPerGame = 0;
            awayGoalsPerGame = 0;
            // get all fixtures before the current fixture
            List<Fixture> home_previous_results = homeTeam.GetFixturesBefore(date);
            double total_goals = 0;

            if (home_previous_results.Count > 0)
            {
                foreach (Fixture fixture in home_previous_results)
                {
                    // add up goals in those fixtures
                    total_goals += fixture.HomeGoals;
                }
                // divide by number of games
                homeGoalsPerGame = (float)total_goals / (float)home_previous_results.Count;
            }

            List<Fixture> away_previous_results = awayTeam.GetFixturesBefore(date);

            if (away_previous_results.Count > 0)
            {
                total_goals = 0;

                foreach (Fixture fixture in away_previous_results)
                {
                    // add up goals in those fixtures
                    total_goals += fixture.HomeGoals;
                }
                // divide by number of games
                awayGoalsPerGame = (float)total_goals / away_previous_results.Count;
            }
        }

        public void PredictResult(double alpha,double beta)
        {
            List<double> homeSample;
            List<double> awaySample;
            List<double> homeOppSample;
            List<double> awayOppSample;

            double lgavghome_goals;
            double lgavgaway_goals;
            double lgavghome_conceded;
            double lgavgaway_conceded;

            CalculateGoalsPerGame();
            HomeForm = homeTeam.CalculateForm(date);
            AwayForm = awayTeam.CalculateForm(date);

            homeSample = homeTeam.CreateHomeSample(date);   // create the samples
            awaySample = awayTeam.CreateAwaySample(date);

			if((homeSample.Count != 0)&&(awaySample.Count != 0))
			{
                lgavghome_goals = league.GetAverageHomeGoals(date);
				lgavgaway_goals = league.GetAverageAwayGoals(date);

				//find the average teams concede at home and away this is the inverse of goals scored

				lgavghome_conceded = lgavgaway_goals;
				lgavgaway_conceded = lgavghome_goals;

                homeOppSample = homeTeam.CreateHomeOppositionSample(date);
                awayOppSample = awayTeam.CreateAwayOppositionSample(date);

                homeSample = HomeWeightingFunction(homeSample);
                awaySample = AwayWeightingFunction(awaySample);

				// calculates a home attacking strength and defence strength
                CalculateStrengths(homeSample, awaySample, homeOppSample, awayOppSample, alpha,beta);

                predictedGoalDifference = predictedHomeGoals - predictedAwayGoals;

                homeResidual = predictedHomeGoals - homeGoals;
                awayResidual = predictedAwayGoals - awayResidual;

                List<double> homeResiduals = homeTeam.GetResiduals(DateTime.Now);
                averageHomeResidual = homeResiduals.Count > 0 ? homeResiduals.Average() : 0.0;

                List<double> awayResiduals = homeTeam.GetResiduals(DateTime.Now);
                averageAwayResidual = awayResiduals.Count > 0 ? awayResiduals.Average() : 0.0;

                CalculateBothToScore();
                CalculateKellyCriterion();

                //// generate a large sample of simulated results
                //simulated_sample = generateSimulatedResults();
                //fixture_it->calculateProfit(simulated_sample,home_goals,away_goals,home_form,away_form); 
            }
		}

        public void CalculateBothToScore()
        {
            // subtract probabilities from 1.0 for the following results: 0-0, 1-0, 2-0, 3- 0 ....., 0-1, 0-2, 0-3....

            bothToScore = 1.0;

            double home_prob_no_goals = StatsLib.poissonPDF(predictedHomeGoals, 0);
            double away_prob_no_goals = StatsLib.poissonPDF(predictedAwayGoals, 0);

            bothToScore -= (home_prob_no_goals * away_prob_no_goals);  // P(A = 0 & B = 0) 
            bothToScore -= ((1 - home_prob_no_goals) * away_prob_no_goals); // P(A != 0 & B = 0) 
            bothToScore -= (home_prob_no_goals * (1 - away_prob_no_goals)); // P(A = 0 & B != 0) 
        }

        //public List<Fixture> generateSimulatedResults()
        //{
        //    List<Fixture> simulated_sample = new List<Fixture>();
        //    const int NUMBER_OF_RESULTS = 5000;

        //            // generate 5000 random poisson variables using the mean goals scored in home sample
        //    Fixture temp;

        //    simulated_sample.Clear();	// ensure previous simulated results are removed

        //    for(int i=0;i<NUMBER_OF_RESULTS;i++)
        //    {
        //        Random rand = new Random();

        //        predicted_home_goals = predicted_home_goals + (*rand.NextDouble());
        //        double sim_home_goals  = alglib.invnormaldistribution(predicted_home_goals);
        //        double sim_away_goals = alglib.invnormaldistribution(predicted_away_goals);
        //        // generate random variables here using the mean goals scored as lambda
        //        temp = new Fixture(league,date,home_team,away_team,sim_home_goals,sim_away_goals,new Referee(""),new List<Bookmaker>());
        //        simulated_sample.Add(temp);
        //    }
        //    return simulated_sample;
        //}

        public void CalculateKellyCriterion()
        {
            if(BestHomeOdds != null)
            {
                double b = BestHomeOdds.HomeOdds - 1;
                double p = HomeWinProbability;
                double q = 1.0 - p;
                KellyCriterionHome = ((b * p) - q) / b;
            }

            if(BestDrawOdds != null)
            {
                double b = BestDrawOdds.DrawOdds - 1;
                double p = DrawProbability;
                double q = 1.0 - p;
                KellyCriterionDraw = ((b * p) - q) / b;
            }

            if(BestAwayOdds != null)
            {
                double b = BestAwayOdds.AwayOdds - 1;
                double p = AwayWinProbability;
                double q = 1.0 - p;
                KellyCriterionAway = ((b * p) - q) / b;
            }
        }

        public void CalculateStrengths(List<double> home_sample, List<double> away_sample, List<double> home_opp_sample, List<double> away_opp_sample,double alpha,double beta)
        {
            double lgavghome_goals = league.GetAverageHomeGoals(date);
            double lgavgaway_goals = league.GetAverageAwayGoals(date);
            double lgavghome_conceded = lgavgaway_goals;
            double lgavgaway_conceded = lgavghome_goals;

            double home_attack_strength;
            double home_defence_strength;

            double away_attack_strength;
            double away_defence_strength;

            
            home_attack_strength = home_sample.Average() / lgavghome_goals;
            home_defence_strength = home_opp_sample.Average() / lgavgaway_goals;
            away_attack_strength = away_sample.Average() / lgavgaway_goals; // calculates away attacking strength
            away_defence_strength = away_opp_sample.Average() / lgavghome_goals;

            // assumes a teams defensive strength and oppositions
            // attacking strength affect each other when calculating predicted goals

            predictedHomeGoals = home_attack_strength * away_defence_strength * lgavghome_goals-alpha;
            predictedAwayGoals = away_attack_strength * home_defence_strength * lgavgaway_goals-beta;
            
        }

        public void FindBestOdds()
        {
            if(odds.Count() > 0)
            {
                BestHomeOdds = odds.First();
                BestDrawOdds = odds.First();
                BestAwayOdds = odds.First();

                foreach (Bookmaker bookie in odds)
                {
                    if (bookie.HomeOdds > BestHomeOdds.HomeOdds)
                    {
                        BestHomeOdds = bookie;
                    }

                    if (bookie.DrawOdds > BestDrawOdds.DrawOdds)
                    {
                        BestDrawOdds = bookie;
                    }

                    if (bookie.AwayOdds > BestAwayOdds.AwayOdds)
                    {
                        BestAwayOdds = bookie;
                    }
                }

                FindArbitrage();
            }
        }

        public void FindArbitrage()
        {
            Arbitrage = 0.0;

            double homeProbability = 1 / BestHomeOdds.HomeOdds;
            double drawProbability = 1 / BestDrawOdds.DrawOdds;
            double awayProbability = 1 / BestAwayOdds.AwayOdds;

            Arbitrage = homeProbability + drawProbability + awayProbability;
        }

        public double CalculateHomeWinProbability()
        {
            double prob = 0.0;
            double hProb, aProb;
            const int MaxGoals = 9;

            for (int h = 0; h < MaxGoals ; h++)
            {
                for (int a = 0;a < h; a++)
                {
                    // calc poisson prob of h goals given rate of predicted home goals
                    // calc poisson prob of a goals given rate of predicted away goals
                    // multiply and add to total probability
                    hProb = StatsLib.poissonPDF(predictedHomeGoals, h);
                    aProb = StatsLib.poissonPDF(predictedAwayGoals, a);
                    prob += hProb * aProb;
                }
            }

            return prob;
        }

        public double CalculateAwayWinProbability()
        {
            double prob = 0.0;
            double hProb, aProb;
            const int MaxGoals = 9;

            for (int a = 0; a < MaxGoals; a++)
            {
                for (int h = 0; h < a; h++)
                {
                    // calc poisson prob of h goals given rate of predicted home goals
                    // calc poisson prob of a goals given rate of predicted away goals
                    // multiply and add to total probability
                    hProb = StatsLib.poissonPDF(predictedHomeGoals, h);
                    aProb = StatsLib.poissonPDF(predictedAwayGoals, a);
                    prob += hProb * aProb;
                }
            }

            return prob;
        }

        public double CalculateDrawProbability()
        {
            double prob = 0.0;
            double hProb, aProb;
            const int MaxGoals = 9;

            for (int x = 0; x < MaxGoals; x++)
            {
                // calc poisson prob of h goals given rate of predicted home goals
                // calc poisson prob of a goals given rate of predicted away goals
                // multiply and add to total probability
                hProb = StatsLib.poissonPDF(predictedHomeGoals, x);
                aProb = StatsLib.poissonPDF(predictedAwayGoals, x);
                prob += hProb * aProb;
            }

            return prob;
        }

        public override string ToString()
        {
            return League.LeagueID + "," + Date.ToString() + "," + HomeTeam.Name + "," + AwayTeam.Name + ","
                + PredictedGoalDifference + "," + PredictedHomeGoals + "," + PredictedAwayGoals + "," + BothToScore;
        }
	}
}
