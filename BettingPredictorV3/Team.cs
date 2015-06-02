using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BettingPredictorV3
{
    public class Team
    {
        private League league { get; set; }
        private String name { get; set;}
        private List<Fixture> fixtures { get; set; }
        private float goals_per_game;   // goals per game prior to the fixture
        private float goals_per_home_game;  // goals per home game prior to the fixture
        private float goals_per_away_game;  // goals per away game prior to the fixture
        public int form { get; set; }   // points won in last 5 games

        public Team(League league,String name)
        {
            this.league = league;
            this.name = name;
            fixtures = new List<Fixture>();
        }

        public String LeagueID
        {
            get
            {
                return league.LeagueID;
            }
        }
        public String Name
        {
            get
            {
                return name;
            }
            set
            {
                this.name = name;
            }
        }

        public int Form
        {
            get
            {
                return form;
            }
            set
            {
                this.form = form;
            }
        }

        public List<Fixture> Fixtures
        {
            get
            {
                return fixtures;
            }
        }

        public void addFixture(Fixture fixture)
        {
            fixtures.Add(fixture);
        }

        public List<Fixture> getFixturesBefore(DateTime date)
        {
            List<Fixture> previous_results = new List<Fixture>();
            foreach (Fixture fixture in fixtures)
            {
                if (fixture.getDate() < date)
                {
                    previous_results.Add(fixture);
                }
            }

            return previous_results;
        }

        public int calculateForm(DateTime date)
        {
            int idx = 0;
            form = 0;

            List<Fixture> previous_results = getFixturesBefore(date);
            previous_results.Reverse();

            foreach (Fixture fixture in previous_results)
            {
                if (idx < 5)
                {
                    if (fixture.getHomeTeam() == this) // if current team is home side
                    {
                        if (fixture.getAwayGoals() < fixture.getHomeGoals())	// home win
                        {
                            form += 3;
                        }
                        else if (fixture.getAwayGoals() == fixture.getHomeGoals()) // draw
                        {
                            form++;
                        }
                    }
                    else // if current team is the away side
                    {
                        if (fixture.getAwayGoals() > fixture.getHomeGoals())	// away win
                        {
                            form += 3;
                        }
                        else if (fixture.getAwayGoals() == fixture.getHomeGoals()) // draw
                        {
                            form++;
                        }
                    }

                    idx++;
                }
            }

            return form;
        }

        public List<double> createHomeSample(DateTime date)
        {
            List<double> sample = new List<double>();

            foreach (Fixture fixture in fixtures)
            {
                if (fixture.getDate() < date)
                {
                    if (fixture.getHomeTeam() == this)
                    {
                        sample.Add(fixture.getHomeGoals());
                    }
                }
            }

            return sample;
        }

        public List<double> createAwaySample(DateTime date)
        {
            List<double> sample = new List<double>();

            foreach (Fixture fixture in fixtures)
            {
                if (fixture.getDate() < date)
                {
                    if (fixture.getAwayTeam() == this)
                    {
                        sample.Add(fixture.getAwayGoals());
                    }
                }
            }

            return sample;
        }
        public List<double> createHomeOppositionSample(DateTime date)
        {
            List<double> sample = new List<double>();

            foreach (Fixture fixture in fixtures)
            {
                if (fixture.getDate() < date)
                {
                    if (fixture.getHomeTeam() == this)
                    {
                        sample.Add(fixture.getAwayGoals());
                    }
                }
            }

            return sample;
        }

        public List<double> createAwayOppositionSample(DateTime date)
        {
            List<double> sample = new List<double>();

            foreach (Fixture fixture in fixtures)
            {
                if (fixture.getDate() < date)
                {
                    if (fixture.getAwayTeam() == this)
                    {
                        sample.Add(fixture.getHomeGoals());
                    }
                }
            }

            return sample;
        }
        public void predictResults(double alpha,double beta)
        {
            foreach (Fixture fixture in fixtures)
            {
                fixture.predictResult(alpha,beta);
                fixture.calculateResiduals();
            }
        }

        public List<double> getHomeResiduals(DateTime date)
        {
            List<double> residuals = new List<double>();
            foreach (Fixture fixture in fixtures)
            {
                if (fixture.getDate() < date)
                {
                    if (fixture.getHomeResidual() != fixture.getHomeGoals())
                    {
                        residuals.Add(fixture.getHomeResidual());
                    }
                }
            }

            return residuals;
        }
        public List<double> getAwayResiduals(DateTime date)
        {
            List<double> residuals = new List<double>();
            foreach (Fixture fixture in fixtures)
            {
                if (fixture.getDate() < date)
                {
                    if (fixture.getHomeResidual() != fixture.getHomeGoals())
                    {
                        residuals.Add(fixture.getAwayResidual());
                    }
                }
            }

            return residuals;
        }

        public List<double> getResiduals(DateTime date)
        {
            List<double> residuals = new List<double>();
            double residual;
            foreach (Fixture fixture in fixtures)
            {
                if (fixture.getDate() < date)
                {
                    if (fixture.getHomeTeam() == this)
                    {
                        residual = fixture.getHomeResidual();
                        if (!Double.IsNaN(residual))
                        {
                            residuals.Add(residual);
                        }
                    }
                    else if (fixture.getAwayTeam() == this)
                    {
                        residual = fixture.getAwayResidual();
                        if(!Double.IsNaN(residual))
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
