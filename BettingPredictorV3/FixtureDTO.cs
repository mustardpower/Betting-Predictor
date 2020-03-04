using BettingPredictorV3.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingPredictorV3
{
    public abstract class FixtureDTO : IDatabaseObject<Fixture>
    {
        public string LeagueCode { get; internal set; }
        public DateTime Date { get; internal set; }
        public string HomeTeamName { get; internal set; }
        public string AwayTeamName { get; internal set; }
        public List<Bookmaker> Odds { get; internal set; }

        public abstract Fixture CreateFixture(League league, Team HomeTeam, Team awayTeam);
        public Fixture AddToDatabase(Database database)
        {
            League league = database.GetLeague(LeagueCode);
            if (league == null)
            {
                League newLeague = new League(LeagueCode);
                database.Leagues.Add(newLeague);
                league = newLeague;
            }

            Team homeTeam = database.GetTeam(LeagueCode, HomeTeamName);
            Team awayTeam = database.GetTeam(LeagueCode, AwayTeamName);

            if (homeTeam == null)
            {
                league.AddTeam(new Team(league, HomeTeamName));
                homeTeam = database.GetTeam(LeagueCode, HomeTeamName);
            }
            if (awayTeam == null)
            {
                league.AddTeam(new Team(league, AwayTeamName));
                awayTeam = database.GetTeam(LeagueCode, AwayTeamName);
            }

            var fixture = CreateFixture(league, homeTeam, awayTeam);
            homeTeam.AddFixture(fixture);
            awayTeam.AddFixture(fixture);
            return fixture;
        }
    }
}
