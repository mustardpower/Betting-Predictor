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
    }
}
