using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BettingPredictorV3.DataStructures
{
    [Serializable]
    public class League
    {

        public League()
        {

        }

        public League(String leagueCode)
        {
            this.LeagueCode = leagueCode;
            Fixtures = new List<Fixture>();
        }

        public int LeagueId { get; set; }

        public String LeagueCode { get; set; }

        public virtual ICollection<Fixture> Fixtures { get; set; }

        public void AddFixture(Fixture fixture)
        {
            Fixtures.Add(fixture);
        }

        public void PredictResults(double alpha, double beta)
        {
            
        }

        public List<Fixture> GetFixtures(DateTime date)
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

        public double GetAverageHomeGoals(DateTime date)
        {
            List<double> sample = new List<double>();
            List<Fixture> fixtures = GetFixtures(date);

            if (fixtures.Count == 0)
            {
                return 0;
            }

            foreach(Fixture fixture in fixtures)
            {
                sample.Add(fixture.HomeGoals);
            }

            return sample.Average();
        }

        public double GetAverageAwayGoals(DateTime date)
        {
            List<double> sample = new List<double>();
            List<Fixture> fixtures = GetFixtures(date);

            if (fixtures.Count == 0)
            {
                return 0;
            }

            foreach (Fixture fixture in fixtures)
            {
                sample.Add(fixture.AwayGoals);
            }

            return sample.Average();
        }

        public List<double> GetHomeResiduals(DateTime date)
        {
            List<double> residuals = new List<double>();

            return residuals;
        }
        public List<double> GetAwayResiduals(DateTime date)
        {
            List<double> residuals = new List<double>();

            return residuals;
        }
    }
}
