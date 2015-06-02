using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BettingPredictorV3
{
    public class League
    {
        private String league_ID;
        private List<Team> teams = new List<Team>();

        public League(String league_ID)
        {
            this.league_ID = league_ID;
        }

        public String LeagueID
        {
            get
            {
                return league_ID;
            }
            set
            {
                league_ID = value;
            }
        }

        public void addTeam(Team team)
        {
            if (teams.Find(x => x.Name == team.Name) == null)
            {
                teams.Add(team);
            }
        }

        public Team getTeam(String name)
        {
            foreach (Team team in teams)
            {
                if (team.Name == name)
                {
                    return team;
                }
            }

            return null;
        }

        public void addFixture(Fixture fixture)
        {
            if (teams.Count(x => x.Name == fixture.getHomeTeam().Name) == 0)
            {
                // if no match found then add team to the league
                addTeam(fixture.getHomeTeam());
            }

            foreach (Team team in teams)
            {
                if (team.Name == fixture.getHomeTeam().Name)
                {
                    team.addFixture(fixture);
                }
            }
        }

        public int getFileOffset(string[] fixture_data)
        {
            if (fixture_data.Length == 52)
            {
                return 0;
            }
            else if(fixture_data.Length == 64)
            {
                return 12;
            }
            else if(fixture_data.Length == 65)
            {
                return 13;
            }
            else
            {
                throw new Exception();
            }
        }

        public void parseHistoricalData(string[] fixture_data)
        {
            Team home_team = null;
            Team away_team = null;
            List<Bookmaker> odds = new List<Bookmaker>();

            var date_params = fixture_data[1].Split('/');
            DateTime date = new DateTime(2000 + int.Parse(date_params[2]), int.Parse(date_params[1]), int.Parse(date_params[0]));
            String home_team_name = fixture_data[2];
            String away_team_name = fixture_data[3];

            for (int idx = 0; idx < fixture_data.Length; idx++)
            {
                if (string.IsNullOrEmpty(fixture_data[idx]))
                {
                    fixture_data[idx] = "0";
                }
            }

            int offset = getFileOffset(fixture_data);

            try
            {
                Bookmaker bet_365 = new Bookmaker("Bet 365", double.Parse(fixture_data[10 + offset]), double.Parse(fixture_data[11 + offset]),
                        double.Parse(fixture_data[12 + offset]));
                Bookmaker bet_win = new Bookmaker("BetWin", double.Parse(fixture_data[13 + offset]), double.Parse(fixture_data[14 + offset]),
                    double.Parse(fixture_data[15 + offset]));
                Bookmaker inter_wetten = new Bookmaker("InterWetten", double.Parse(fixture_data[16 + offset]), double.Parse(fixture_data[17 + offset]),
                    double.Parse(fixture_data[18 + offset]));
                Bookmaker ladbrokes = new Bookmaker("Ladbrokes", double.Parse(fixture_data[19 + offset]), double.Parse(fixture_data[20 + offset]),
                    double.Parse(fixture_data[21 + offset]));
                Bookmaker pinnacle_sport = new Bookmaker("Pinnacle Sport", double.Parse(fixture_data[22 + offset]), double.Parse(fixture_data[23 + offset]),
                    double.Parse(fixture_data[24 + offset]));
                Bookmaker william_hill = new Bookmaker("William Hill", double.Parse(fixture_data[25 + offset]), double.Parse(fixture_data[26 + offset]),
                    double.Parse(fixture_data[27 + offset]));
                Bookmaker stan_james = new Bookmaker("Stan James", double.Parse(fixture_data[28 + offset]), double.Parse(fixture_data[29 + offset]),
                    double.Parse(fixture_data[30 + offset]));

                /*odds.Add(bet_365);
                odds.Add(bet_win);
                odds.Add(inter_wetten);
                odds.Add(ladbrokes);
                odds.Add(pinnacle_sport);*/
                odds.Add(william_hill);
                //odds.Add(stan_james);
            }
            catch (FormatException ex)
            {
                String a = ex.Message;
            }

            int home_goals = int.Parse(fixture_data[4]);
            int away_goals = int.Parse(fixture_data[5]);

            addTeam(new Team(this, home_team_name));
            addTeam(new Team(this, away_team_name));

            home_team = getTeam(home_team_name);
            away_team = getTeam(away_team_name);

            Fixture newFixture = new Fixture(this, date, home_team, away_team, home_goals, away_goals, new Referee(""), odds);
            home_team.addFixture(newFixture);
            away_team.addFixture(newFixture);
        }

        public void predictResults(double alpha, double beta)
        {
            foreach (Team team in teams)
            {
                team.predictResults(alpha,beta);
            }
        }

        public List<Fixture> getFixtures()
        {
            List<Fixture> fixtures = new List<Fixture>();
            foreach (Team team in teams)
            {
                fixtures.AddRange(team.Fixtures);
            }

            return fixtures;
        }

        public List<Fixture> getFixtures(DateTime date)
        {
            List<Fixture> fixtures = new List<Fixture>();
            foreach (Team team in teams)
            {
                fixtures.AddRange(team.getFixturesBefore(date));
            }

            return fixtures;
        }

        public double getAverageHomeGoals(DateTime date)
        {
            List<double> sample = new List<double>();
            List<Fixture> fixtures = getFixtures(date);

            if (fixtures.Count == 0)
            {
                return 0;
            }

            foreach(Fixture fixture in fixtures)
            {
                sample.Add(fixture.getHomeGoals());
            }

            return sample.Average();
        }

        public double getAverageAwayGoals(DateTime date)
        {
            List<double> sample = new List<double>();
            List<Fixture> fixtures = getFixtures(date);

            if (fixtures.Count == 0)
            {
                return 0;
            }

            foreach (Fixture fixture in fixtures)
            {
                sample.Add(fixture.getAwayGoals());
            }

            return sample.Average();
        }

        public List<double> getHomeResiduals(DateTime date)
        {
            List<double> residuals = new List<double>();
            foreach (Team team in teams)
            {
                residuals.AddRange(team.getHomeResiduals(date));
            }

            return residuals;
        }
        public List<double> getAwayResiduals(DateTime date)
        {
            List<double> residuals = new List<double>();
            foreach (Team team in teams)
            {
                residuals.AddRange(team.getAwayResiduals(date));
            }

            return residuals;
        }
    }
}
