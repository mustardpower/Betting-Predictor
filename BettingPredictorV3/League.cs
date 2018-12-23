using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BettingPredictorV3.DataStructures
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

        public void AddTeam(Team team)
        {
            if (teams.Find(x => x.Name == team.Name) == null)
            {
                teams.Add(team);
            }
        }

        public Team GetTeam(String name)
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

        public void AddFixture(Fixture fixture)
        {
            if (teams.Count(x => x.Name == fixture.HomeTeam.Name) == 0)
            {
                // if no match found then add team to the league
                AddTeam(fixture.HomeTeam);
            }

            foreach (Team team in teams)
            {
                if (team.Name == fixture.HomeTeam.Name)
                {
                    team.AddFixture(fixture);
                }
            }
        }

        public int GetFileOffset(string[] fixture_data)
        {
            if (fixture_data.Length == 49 || fixture_data.Length == 52)
            {
                return 0;
            }
            else if(fixture_data.Length == 64)
            {
                return 12;
            }
            else if(fixture_data.Length == 62 || fixture_data.Length == 65)
            {
                return 13;
            }
            else if(fixture_data.Length == 19)
            {
                return 0;
            }
            else
            {
                return fixture_data.Length - 52;
            }
        }

        public void ParseHistoricalData(string[] fixture_data)
        {
            Team home_team = null;
            Team away_team = null;
            List<Bookmaker> odds = new List<Bookmaker>();

            /* Additional league data has been added at a later point, 
            these new leagues have less info in a different format */
            var newLeague = fixture_data.Length == 19; 
            var dateIndex = newLeague ? 3 : 1;
            var date_params = fixture_data[dateIndex].Split('/');
            int yearParam = int.Parse(date_params[2]);
            if(!newLeague) { yearParam += 2000; }
            DateTime date = new DateTime(yearParam, int.Parse(date_params[1]), int.Parse(date_params[0]));
            String home_team_name = newLeague ? fixture_data[5] : fixture_data[2];
            String away_team_name = newLeague ? fixture_data[6] : fixture_data[3];

            for (int idx = 0; idx < fixture_data.Length; idx++)
            {
                if (string.IsNullOrEmpty(fixture_data[idx]))
                {
                    fixture_data[idx] = "0";
                }
            }

            int offset = GetFileOffset(fixture_data);

            try
            {
                List<Bookmaker> bookmakers = new List<Bookmaker>();
                if (newLeague)
                {
                    bookmakers.Add(new Bookmaker("Best Odds", double.Parse(fixture_data[10 + offset]), double.Parse(fixture_data[11 + offset]),
                            double.Parse(fixture_data[12 + offset])));
                }
                else
                {
                    bookmakers.Add(new Bookmaker("Bet 365", double.Parse(fixture_data[10 + offset]), double.Parse(fixture_data[11 + offset]),
                            double.Parse(fixture_data[12 + offset])));
                    bookmakers.Add(new Bookmaker("BetWin", double.Parse(fixture_data[13 + offset]), double.Parse(fixture_data[14 + offset]),
                        double.Parse(fixture_data[15 + offset])));
                    bookmakers.Add(new Bookmaker("InterWetten", double.Parse(fixture_data[16 + offset]), double.Parse(fixture_data[17 + offset]),
                        double.Parse(fixture_data[18 + offset])));
                    bookmakers.Add(new Bookmaker("Ladbrokes", double.Parse(fixture_data[19 + offset]), double.Parse(fixture_data[20 + offset]),
                        double.Parse(fixture_data[21 + offset])));
                    bookmakers.Add(new Bookmaker("Pinnacle Sport", double.Parse(fixture_data[22 + offset]), double.Parse(fixture_data[23 + offset]),
                        double.Parse(fixture_data[24 + offset])));
                    bookmakers.Add(new Bookmaker("William Hill", double.Parse(fixture_data[25 + offset]), double.Parse(fixture_data[26 + offset]),
                        double.Parse(fixture_data[27 + offset])));
                    bookmakers.Add(new Bookmaker("Stan James", double.Parse(fixture_data[28 + offset]), double.Parse(fixture_data[29 + offset]),
                        double.Parse(fixture_data[30 + offset])));
                }

                foreach (Bookmaker bookmaker in bookmakers)
                {
                    int index = DatabaseSettings.BookmakersUsed.IndexOf(bookmaker.Name);
                    if (index != -1)
                    {
                        odds.Add(bookmaker);
                    }
                }
            }
            catch (FormatException ex)
            {
                String a = ex.Message;
            }

            int home_goals;
            int away_goals;

            if(newLeague)
            {
                home_goals = int.Parse(fixture_data[7]);
                away_goals = int.Parse(fixture_data[8]);
            }
            else
            {
                home_goals = int.Parse(fixture_data[4]);
                away_goals = int.Parse(fixture_data[5]);
            }
            

            AddTeam(new Team(this, home_team_name));
            AddTeam(new Team(this, away_team_name));

            home_team = GetTeam(home_team_name);
            away_team = GetTeam(away_team_name);

            Fixture newFixture = new Fixture(this, date, home_team, away_team, home_goals, away_goals, new Referee(""), odds);
            home_team.AddFixture(newFixture);
            away_team.AddFixture(newFixture);
        }

        public void PredictResults(double alpha, double beta)
        {
            foreach (Team team in teams)
            {
                team.PredictResults(alpha,beta);
            }
        }

        public List<Fixture> GetFixtures()
        {
            List<Fixture> fixtures = new List<Fixture>();
            foreach (Team team in teams)
            {
                fixtures.AddRange(team.Fixtures);
            }

            return fixtures;
        }

        public List<Fixture> GetFixtures(DateTime date)
        {
            List<Fixture> fixtures = new List<Fixture>();
            foreach (Team team in teams)
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
            foreach (Team team in teams)
            {
                residuals.AddRange(team.GetHomeResiduals(date));
            }

            return residuals;
        }
        public List<double> GetAwayResiduals(DateTime date)
        {
            List<double> residuals = new List<double>();
            foreach (Team team in teams)
            {
                residuals.AddRange(team.GetAwayResiduals(date));
            }

            return residuals;
        }
    }
}
