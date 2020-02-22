using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using BettingPredictorV3.DataStructures;

namespace BettingPredictorV3
{
    public class FootballResultsDbContext : DbContext
    {
        public DbSet<League> Leagues { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Fixture> Fixtures { get; set; }

        public DbSet<Bookmaker> Bookmakers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Fixture>()
                        .HasRequired(m => m.HomeTeam)
                        .WithMany(t => t.HomeFixtures)
                        .HasForeignKey(m => m.HomeTeamId)
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<Fixture>()
                        .HasRequired(m => m.AwayTeam)
                        .WithMany(t => t.AwayFixtures)
                        .HasForeignKey(m => m.AwayTeamId)
                        .WillCascadeOnDelete(false);
        }
    }
}
