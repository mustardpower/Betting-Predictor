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
    }

}
