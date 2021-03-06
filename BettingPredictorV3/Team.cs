﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BettingPredictorV3.DataStructures
{
    [Serializable]
    public class Team
    {
        private League league;
        private String name;
        private List<Fixture> fixtures;
        private int form; // points won in last 5 games

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
                this.name = value;
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
                this.form = value;
            }
        }

        public List<Fixture> Fixtures
        {
            get
            {
                return fixtures;
            }
        }

        public void AddFixture(Fixture fixture)
        {
            fixtures.Add(fixture);
        }

        public List<Fixture> GetFixturesBefore(DateTime date)
        {
            List<Fixture> previous_results = new List<Fixture>();
            foreach (Fixture fixture in fixtures)
            {
                if (fixture.Date < date)
                {
                    previous_results.Add(fixture);
                }
            }

            return previous_results;
        }

        public int CalculateForm(DateTime date)
        {
            int idx = 0;
            form = 0;
            const int kNumberOfRelevantGames  = 5;
            const int kNumberOfPtsForWin = 3;

            List<Fixture> previous_results = GetFixturesBefore(date);
            previous_results.Reverse();

            foreach (Fixture fixture in previous_results)
            {
                if (idx < kNumberOfRelevantGames)
                {
                    if (fixture.HomeTeam == this) // if current team is home side
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

        public List<double> CreateHomeSample(DateTime date)
        {
            List<double> sample = new List<double>();

            foreach (Fixture fixture in fixtures)
            {
                if (fixture.Date < date)
                {
                    if (fixture.HomeTeam == this)
                    {
                        sample.Add(fixture.HomeGoals);
                    }
                }
            }

            return sample;
        }

        public List<double> CreateAwaySample(DateTime date)
        {
            List<double> sample = new List<double>();

            foreach (Fixture fixture in fixtures)
            {
                if (fixture.Date < date)
                {
                    if (fixture.AwayTeam == this)
                    {
                        sample.Add(fixture.AwayGoals);
                    }
                }
            }

            return sample;
        }
        public List<double> CreateHomeOppositionSample(DateTime date)
        {
            List<double> sample = new List<double>();

            foreach (Fixture fixture in fixtures)
            {
                if (fixture.Date < date)
                {
                    if (fixture.HomeTeam == this)
                    {
                        sample.Add(fixture.AwayGoals);
                    }
                }
            }

            return sample;
        }

        public List<double> CreateAwayOppositionSample(DateTime date)
        {
            List<double> sample = new List<double>();

            foreach (Fixture fixture in fixtures)
            {
                if (fixture.Date < date)
                {
                    if (fixture.AwayTeam == this)
                    {
                        sample.Add(fixture.HomeGoals);
                    }
                }
            }

            return sample;
        }
        public void PredictResults()
        {
            ResultPredictor resultPredictor = new ResultPredictor();
            foreach (Fixture fixture in fixtures)
            {
                resultPredictor.PredictResult(fixture);
                fixture.CalculateResiduals();
            }
        }

        public List<double> GetHomeResiduals(DateTime date)
        {
            List<double> residuals = new List<double>();
            foreach (Fixture fixture in fixtures)
            {
                if (fixture.Date < date)
                {
                    if (fixture.HomeResidual != fixture.HomeGoals)
                    {
                        residuals.Add(fixture.HomeResidual);
                    }
                }
            }

            return residuals;
        }
        public List<double> GetAwayResiduals(DateTime date)
        {
            List<double> residuals = new List<double>();
            foreach (Fixture fixture in fixtures)
            {
                if (fixture.Date < date)
                {
                    if (fixture.HomeResidual != fixture.HomeGoals)
                    {
                        residuals.Add(fixture.AwayResidual);
                    }
                }
            }

            return residuals;
        }

        public List<double> GetResiduals(DateTime date)
        {
            List<double> residuals = new List<double>();
            double residual;
            foreach (Fixture fixture in fixtures)
            {
                if (fixture.Date < date)
                {
                    if (fixture.HomeTeam == this)
                    {
                        residual = fixture.HomeResidual;
                        if (!Double.IsNaN(residual))
                        {
                            residuals.Add(residual);
                        }
                    }
                    else if (fixture.AwayTeam == this)
                    {
                        residual = fixture.AwayResidual;
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
