using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace BettingPredictorV3.DataStructures
{
    [Serializable]
    public class Fixture
    {
        public int FixtureId { get; set; }

        public int LeagueId { get; set; }

        public int HomeTeamId { get; set; }

        public int AwayTeamId { get; set; }
        
        private double predictedHomeGoals;
        private double predictedAwayGoals;
        private double? homeWinProbabilty;
        private double? drawProbability;
        private double? awayWinProbability;

        public int HomeForm { get; set; }
        public int AwayForm { get; set; }

        public virtual ICollection<Bookmaker> Odds { get; set; }

        public Fixture()
        {
        }

        public Fixture(League league, DateTime date, Team home_team, Team away_team, Referee referee,List<Bookmaker> odds) // constructor for fixture
        {
            this.FixtureLeague = league;
            this.Date = date;
            this.HomeTeam = home_team;
            this.AwayTeam = away_team;
            this.Referee = referee;
            this.Odds = odds;
            predictedHomeGoals = 0;
            predictedAwayGoals = 0;
            BothToScore = 0.0;

            FindBestOdds();
        }
        public Fixture(League league,DateTime date,Team home_team,Team away_team,double home_goals,double away_goals,Referee referee,List<Bookmaker> odds) // for result
        {
            this.FixtureLeague = league;
            this.Date = date;
            this.HomeTeam = home_team;
            this.AwayTeam = away_team;
            this.HomeGoals = home_goals;
            this.AwayGoals = away_goals;
            this.Referee = referee;
            this.Odds = odds;
            predictedHomeGoals = 0;
            predictedAwayGoals = 0;

            FindBestOdds();
        }

        [ForeignKey("LeagueId")]
        public virtual League FixtureLeague { get; set; }

        [ForeignKey("HomeTeamId")]
        public virtual Team HomeTeam { get; set; }

        [ForeignKey("AwayTeamId")]
        public virtual Team AwayTeam { get; set; }

        public double HomeGoals { get; set; }

        public double AwayGoals { get; set; }

        public double HomeResidual { get; set; }

        public double AwayResidual { get; set; }

        public DateTime Date { get; set; }

        // don't bother mapping these as they can be calculated from the odds list
        [NotMapped]
        public Bookmaker BestHomeOdds { get; set; }

        [NotMapped]
        public Bookmaker BestDrawOdds { get; set; }

        [NotMapped]
        public Bookmaker BestAwayOdds { get; set; }

        public double PredictedHomeGoals { get; set; }

        public double PredictedAwayGoals { get; set; }

        public double BothToScore { get; set; }

        public Referee Referee { get; set; }

        public double HomeGoalsPerGame { get; set; }

        public double AwayGoalsPerGame { get; set; }

        public double PredictedGoalDifference { get; set; }
        public double AverageHomeResidual { get; set; }
        public double AverageAwayResidual { get; set; }
        
        [NotMapped]
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

        public void CalculateResiduals()
        {
            HomeResidual = HomeGoals - predictedHomeGoals;
            AwayResidual = AwayGoals - predictedAwayGoals;
        }

        public void CalculateGoalsPerGame(List<Fixture> previousHomeFixtures, List<Fixture> previousAwayFixtures)
        {
            HomeGoalsPerGame = 0;
            AwayGoalsPerGame = 0;
            double total_goals = 0;

            if (previousHomeFixtures.Count > 0)
            {
                foreach (Fixture fixture in previousHomeFixtures)
                {
                    // add up goals in those fixtures
                    total_goals += fixture.HomeGoals;
                }
                // divide by number of games
                HomeGoalsPerGame = (float)total_goals / (float)previousHomeFixtures.Count;
            }

            if (previousAwayFixtures.Count > 0)
            {
                total_goals = 0;

                foreach (Fixture fixture in previousAwayFixtures)
                {
                    // add up goals in those fixtures
                    total_goals += fixture.HomeGoals;
                }
                // divide by number of games
                AwayGoalsPerGame = (float)total_goals / previousAwayFixtures.Count;
            }
        }

        public void CalculateBothToScore()
        {
            // subtract probabilities from 1.0 for the following results: 0-0, 1-0, 2-0, 3- 0 ....., 0-1, 0-2, 0-3....

            BothToScore = 1.0;

            double home_prob_no_goals = StatsLib.poissonPDF(predictedHomeGoals, 0);
            double away_prob_no_goals = StatsLib.poissonPDF(predictedAwayGoals, 0);

            BothToScore -= (home_prob_no_goals * away_prob_no_goals);  // P(A = 0 & B = 0) 
            BothToScore -= ((1 - home_prob_no_goals) * away_prob_no_goals); // P(A != 0 & B = 0) 
            BothToScore -= (home_prob_no_goals * (1 - away_prob_no_goals)); // P(A = 0 & B != 0) 
        }

        public void CalculateKellyCriterion()
        {
            if(BestHomeOdds != null)
            {
                double b = BestHomeOdds.HomeOdds - 1;
                double p = CalculateHomeWinProbability();
                double q = 1.0 - p;
                KellyCriterionHome = ((b * p) - q) / b;
            }

            if(BestDrawOdds != null)
            {
                double b = BestDrawOdds.DrawOdds - 1;
                double p = CalculateDrawProbability();
                double q = 1.0 - p;
                KellyCriterionDraw = ((b * p) - q) / b;
            }

            if(BestAwayOdds != null)
            {
                double b = BestAwayOdds.AwayOdds - 1;
                double p = CalculateAwayWinProbability();
                double q = 1.0 - p;
                KellyCriterionAway = ((b * p) - q) / b;
            }
        }

        public void FindBestOdds()
        {
            if(Odds.Count() > 0)
            {
                BestHomeOdds = Odds.First();
                BestDrawOdds = Odds.First();
                BestAwayOdds = Odds.First();

                foreach (Bookmaker bookie in Odds)
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
                    hProb = StatsLib.poissonPDF(PredictedHomeGoals, h);
                    aProb = StatsLib.poissonPDF(PredictedAwayGoals, a);
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
                    hProb = StatsLib.poissonPDF(PredictedHomeGoals, h);
                    aProb = StatsLib.poissonPDF(PredictedAwayGoals, a);
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
                hProb = StatsLib.poissonPDF(PredictedHomeGoals, x);
                aProb = StatsLib.poissonPDF(PredictedAwayGoals, x);
                prob += hProb * aProb;
            }

            return prob;
        }

        public override string ToString()
        {
            return FixtureLeague.LeagueCode + "," + Date.ToString() + "," + HomeTeam.Name + "," + AwayTeam.Name + ","
                + PredictedGoalDifference + "," + PredictedHomeGoals + "," + PredictedAwayGoals + "," + BothToScore;
        }
	}
}
