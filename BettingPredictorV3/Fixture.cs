using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BettingPredictorV3.DataStructures
{
    public class Fixture
    {
        public League league { get; set; }
        public DateTime date { get; set; }
        public Team home_team { get; set; }
        public Team away_team { get; set; }
        private double home_goals;
        private double away_goals;
        public Referee referee { get; set; }
        public float home_goals_per_game { get; set; }
        public float away_goals_per_game { get; set; }

        public double predicted_home_goals { get; set; }
        public double predicted_away_goals { get; set; }
        public double predicted_goal_difference { get; set; }
        public double home_residual;
        private double away_residual;
        public double average_home_residual { get; set; }
        public double average_away_residual { get; set; }
        public double both_to_score { get; set; }   // probability that both teams score in the fixture

        public int home_form { get; set; }
        public int away_form { get; set; }

        public List<Bookmaker> odds;
        public Bookmaker best_home_odds { get; set; }
        public Bookmaker best_draw_odds { get; set; }
        public Bookmaker best_away_odds { get; set; }

        public Fixture()
        {
        }

        public Fixture(League league, DateTime date, Team home_team, Team away_team, Referee referee,List<Bookmaker> odds) // constructor for fixture
        {
            this.league = league;
            this.date = date;
            this.home_team = home_team;
            this.away_team = away_team;
            this.referee = referee;
            this.odds = odds;
            predicted_home_goals = 0;
            predicted_away_goals = 0;
            both_to_score = 0.0;

            FindBestOdds();
        }
        public Fixture(League league,DateTime date,Team home_team,Team away_team,double home_goals,double away_goals,Referee referee,List<Bookmaker> odds) // for result
        {
            this.league = league;
            this.date = date;
            this.home_team = home_team;
            this.away_team = away_team;
            this.home_goals = home_goals;
            this.away_goals = away_goals;
            this.referee = referee;
            this.odds = odds;
            predicted_home_goals = 0;
            predicted_away_goals = 0;

            FindBestOdds();
        }

        public Team HomeTeam
        {
            get
            {
                return home_team;
            }
        }

        public Team AwayTeam
        {
            get
            {
                return away_team;
            }
        }

        public double HomeGoals
        {
            get
            {
                return home_goals;
            }
        }

        public double AwayGoals
        {
            get
            {
                return away_goals;
            }
        }

        public double HomeResidual
        {
            get
            {
                return home_residual;
            }
        }

        public double AwayResidual
        {
            get
            {
                return away_residual;
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
            home_residual = home_goals - predicted_home_goals;
            away_residual = away_goals - predicted_away_goals;
        }

        public void CalculateGoalsPerGame()
        {
            home_goals_per_game = 0;
            away_goals_per_game = 0;
            // get all fixtures before the current fixture
            List<Fixture> home_previous_results = home_team.GetFixturesBefore(date);
            double total_goals = 0;

            if (home_previous_results.Count > 0)
            {
                foreach (Fixture fixture in home_previous_results)
                {
                    // add up goals in those fixtures
                    total_goals += fixture.HomeGoals;
                }
                // divide by number of games
                home_goals_per_game = (float)total_goals / (float)home_previous_results.Count;
            }

            List<Fixture> away_previous_results = away_team.GetFixturesBefore(date);

            if (away_previous_results.Count > 0)
            {
                total_goals = 0;

                foreach (Fixture fixture in away_previous_results)
                {
                    // add up goals in those fixtures
                    total_goals += fixture.HomeGoals;
                }
                // divide by number of games
                away_goals_per_game = (float)total_goals / away_previous_results.Count;
            }
        }

        public void PredictResult(double alpha,double beta)
        {
            List<double> home_sample;
            List<double> away_sample;
            List<double> home_opp_sample;
            List<double> away_opp_sample;

            double lgavghome_goals;
            double lgavgaway_goals;
            double lgavghome_conceded;
            double lgavgaway_conceded;

            CalculateGoalsPerGame();
            home_form = home_team.CalculateForm(date);
            away_form = away_team.CalculateForm(date);
    
			home_sample = home_team.CreateHomeSample(date);	// create the samples
			away_sample = away_team.CreateAwaySample(date);

			if((home_sample.Count != 0)&&(away_sample.Count != 0))
			{
                lgavghome_goals = league.GetAverageHomeGoals(date);
				lgavgaway_goals = league.GetAverageAwayGoals(date);

				//find the average teams concede at home and away this is the inverse of goals scored

				lgavghome_conceded = lgavgaway_goals;
				lgavgaway_conceded = lgavghome_goals;

				home_opp_sample = home_team.CreateHomeOppositionSample(date);
				away_opp_sample = away_team.CreateAwayOppositionSample(date);

                home_sample = HomeWeightingFunction(home_sample);
                away_sample = AwayWeightingFunction(away_sample);

				// calculates a home attacking strength and defence strength
                CalculateStrengths(home_sample, away_sample, home_opp_sample, away_opp_sample,alpha,beta);

                predicted_goal_difference = predicted_home_goals - predicted_away_goals;

                home_residual = predicted_home_goals - home_goals;
                away_residual = predicted_away_goals - away_goals;

                average_home_residual = home_team.GetResiduals(DateTime.Now).Average(); 
                average_away_residual = away_team.GetResiduals(DateTime.Now).Average();

                CalculateBothToScore();
                //// generate a large sample of simulated results
                //simulated_sample = generateSimulatedResults();
                //fixture_it->calculateProfit(simulated_sample,home_goals,away_goals,home_form,away_form);
			}
		}

        public void CalculateBothToScore()
        {
            // subtract probabilities from 1.0 for the following results: 0-0, 1-0, 2-0, 3- 0 ....., 0-1, 0-2, 0-3....

            both_to_score = 1.0;

            double home_prob_no_goals = StatsLib.poissonPDF(predicted_home_goals, 0);
            double away_prob_no_goals = StatsLib.poissonPDF(predicted_away_goals, 0);

            both_to_score -= (home_prob_no_goals * away_prob_no_goals);  // P(A = 0 & B = 0) 
            both_to_score -= ((1 - home_prob_no_goals) * away_prob_no_goals); // P(A != 0 & B = 0) 
            both_to_score -= (home_prob_no_goals * (1 - away_prob_no_goals)); // P(A = 0 & B != 0) 
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

            predicted_home_goals = home_attack_strength * away_defence_strength * lgavghome_goals-alpha;
            predicted_away_goals = away_attack_strength * home_defence_strength * lgavgaway_goals-beta;
            
        }

        public void FindBestOdds()
        {
            double home_odds = 0 ;
            double draw_odds = 0 ;
            double away_odds = 0 ;
            foreach (Bookmaker bookie in odds)
            {
                if (bookie.HomeOdds > home_odds)
                {
                    home_odds = bookie.HomeOdds;
                    best_home_odds = bookie;
                }

                if (bookie.DrawOdds > draw_odds)
                {
                    draw_odds = bookie.DrawOdds;
                    best_draw_odds = bookie;
                }

                if (bookie.AwayOdds > away_odds)
                {
                    away_odds = bookie.AwayOdds;
                    best_away_odds = bookie;
                }
            }
        }

        public double HomeWinProbability()
        {
            double prob = 0.0;
            double h_prob, a_prob;
            const int MAX_GOALS = 9;

            for (int h = 0; h < MAX_GOALS ; h++)
            {
                for (int a = 0;a < h; a++)
                {
                    // calc poisson prob of h goals given rate of predicted home goals
                    // calc poisson prob of a goals given rate of predicted away goals
                    // multiply and add to total probability
                    h_prob = StatsLib.poissonPDF(predicted_home_goals, h);
                    a_prob = StatsLib.poissonPDF(predicted_away_goals, a);
                    prob += h_prob * a_prob;
                }
            }

            return prob;
        }

        public double AwayWinProbability()
        {
            double prob = 0.0;
            double h_prob, a_prob;
            const int MAX_GOALS = 9;

            for (int a = 0; a < MAX_GOALS; a++)
            {
                for (int h = 0; h < a; h++)
                {
                    // calc poisson prob of h goals given rate of predicted home goals
                    // calc poisson prob of a goals given rate of predicted away goals
                    // multiply and add to total probability
                    h_prob = StatsLib.poissonPDF(predicted_home_goals, h);
                    a_prob = StatsLib.poissonPDF(predicted_away_goals, a);
                    prob += h_prob * a_prob;
                }
            }

            return prob;
        }

        public double DrawProbability()
        {
            double prob = 0.0;
            double h_prob, a_prob;
            const int MAX_GOALS = 9;

            for (int x = 0; x < MAX_GOALS; x++)
            {
                // calc poisson prob of h goals given rate of predicted home goals
                // calc poisson prob of a goals given rate of predicted away goals
                // multiply and add to total probability
                h_prob = StatsLib.poissonPDF(predicted_home_goals, x);
                a_prob = StatsLib.poissonPDF(predicted_away_goals, x);
                prob += h_prob * a_prob;
            }

            return prob;
        }
	}
}
