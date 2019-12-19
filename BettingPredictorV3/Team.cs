using System;
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

        public Team(League league,String name)
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

        public virtual ICollection<Fixture> Fixtures
        {
            get
            {
                return HomeFixtures.Concat(AwayFixtures).ToList();
            }
        }

        public void AddFixture(Fixture fixture)
        {
            Fixtures.Add(fixture);
        }

        public List<Fixture> GetFixturesBefore(DateTime date)
        {
            List<Fixture> previous_results = new List<Fixture>();
            foreach (Fixture fixture in Fixtures)
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
