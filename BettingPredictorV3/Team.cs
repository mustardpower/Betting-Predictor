﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace BettingPredictorV3.DataStructures
{
    [Serializable]
    public class Team
    {
        public int TeamId { get; set; }

        private String name;
        private int form; // points won in last 5 games

        [InverseProperty("HomeTeam")]
        public virtual ICollection<Fixture> HomeFixtures { get; set; }

        [InverseProperty("AwayTeam")]
        public virtual ICollection<Fixture> AwayFixtures { get; set; }

        public Team()
        {
            Name = "Default Name";
            HomeFixtures = new List<Fixture>();
            AwayFixtures = new List<Fixture>();
        }
        public Team(String name)
        {
            this.name = name;
            HomeFixtures = new List<Fixture>();
            AwayFixtures = new List<Fixture>();
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

        [NotMapped]
        public virtual ICollection<Fixture> Fixtures
        {
            get
            {
                using(var db = new FootballResultsDbContext())
                {
                    Team teamInDatabase = db.Teams.Where(team => TeamId == TeamId).FirstOrDefault();
                    var combinedFixtures = teamInDatabase.HomeFixtures.Concat(teamInDatabase.AwayFixtures).ToList();
                    return combinedFixtures;
                }
            }
        }

        public void AddFixture(Fixture fixture)
        {
            using(var db = new FootballResultsDbContext())
            {
                db.Fixtures.Add(fixture);
                db.SaveChanges();
            }
        }

        public List<Fixture> GetFixturesBefore(DateTime date)
        {
            using (var db = new FootballResultsDbContext())
            {
                var homeFixtures = db.Fixtures.Where(fixture => fixture.Date < date && fixture.HomeTeamId == TeamId);
                var awayFixtures = db.Fixtures.Where(fixture => fixture.Date < date && fixture.AwayTeamId == TeamId);
                return homeFixtures.Concat(awayFixtures).ToList();
            }
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

            foreach (Fixture fixture in Fixtures)
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

            foreach (Fixture fixture in Fixtures)
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

            foreach (Fixture fixture in Fixtures)
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

            foreach (Fixture fixture in Fixtures)
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
        public void PredictResults(double alpha,double beta)
        {
            ResultPredictor resultPredictor = new ResultPredictor();
            foreach (Fixture fixture in Fixtures)
            {
                resultPredictor.PredictResult(fixture, alpha, beta);
                fixture.CalculateResiduals();
            }
        }

        public List<double> GetHomeResiduals(DateTime date)
        {
            List<double> residuals = new List<double>();
            foreach (Fixture fixture in Fixtures)
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
            foreach (Fixture fixture in Fixtures)
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
            foreach (Fixture fixture in Fixtures)
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
