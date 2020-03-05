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
        private string leagueID;

        public League(string leagueID)
        {
            this.leagueID = leagueID;
            Teams = new List<Team>();
        }

        public string LeagueID
        {
            get
            {
                return leagueID;
            }
            set
            {
                leagueID = value;
            }
        }

        public List<Team> Teams { get; set; }

        public void AddTeam(Team team)
        {
            if (Teams.Find(x => x.Name == team.Name) == null)
            {
                Teams.Add(team);
            }
        }

        public Team GetTeam(string name)
        {
            foreach (Team team in Teams)
            {
                if (team.Name == name)
                {
                    return team;
                }
            }

            return null;
        }

        public void AddFixture(Fixture fixture)
        {
            if (Teams.Count(x => x.Name == fixture.HomeTeam.Name) == 0)
            {
                // if no match found then add team to the league
                AddTeam(fixture.HomeTeam);
            }

            foreach (Team team in Teams)
            {
                if (team.Name == fixture.HomeTeam.Name)
                {
                    team.AddFixture(fixture);
                }
            }
        }

        public void PredictResults()
        {
            foreach (Team team in Teams)
            {
                team.PredictResults();
            }
        }

        public List<Fixture> GetFixtures()
        {
            List<Fixture> fixtures = new List<Fixture>();
            foreach (Team team in Teams)
            {
                fixtures.AddRange(team.Fixtures);
            }

            return fixtures;
        }

        public List<Fixture> GetFixtures(DateTime date)
        {
            List<Fixture> fixtures = new List<Fixture>();
            foreach (Team team in Teams)
            {
                fixtures.AddRange(team.GetFixturesBefore(date));
            }

            return fixtures;
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
            foreach (Team team in Teams)
            {
                residuals.AddRange(team.GetHomeResiduals(date));
            }

            return residuals;
        }
        public List<double> GetAwayResiduals(DateTime date)
        {
            List<double> residuals = new List<double>();
            foreach (Team team in Teams)
            {
                residuals.AddRange(team.GetAwayResiduals(date));
            }

            return residuals;
        }
    }
}
