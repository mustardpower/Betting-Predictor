namespace BettingPredictorV3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveLeagueReference : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Bookmakers",
                c => new
                    {
                        BookmakerId = c.Int(nullable: false, identity: true),
                        Fixture_FixtureId = c.Int(),
                    })
                .PrimaryKey(t => t.BookmakerId)
                .ForeignKey("dbo.Fixtures", t => t.Fixture_FixtureId)
                .Index(t => t.Fixture_FixtureId);
            
            AddColumn("dbo.Fixtures", "BestAwayOdds_BookmakerId", c => c.Int());
            AddColumn("dbo.Fixtures", "BestDrawOdds_BookmakerId", c => c.Int());
            AddColumn("dbo.Fixtures", "BestHomeOdds_BookmakerId", c => c.Int());
            CreateIndex("dbo.Fixtures", "BestAwayOdds_BookmakerId");
            CreateIndex("dbo.Fixtures", "BestDrawOdds_BookmakerId");
            CreateIndex("dbo.Fixtures", "BestHomeOdds_BookmakerId");
            AddForeignKey("dbo.Fixtures", "BestAwayOdds_BookmakerId", "dbo.Bookmakers", "BookmakerId");
            AddForeignKey("dbo.Fixtures", "BestDrawOdds_BookmakerId", "dbo.Bookmakers", "BookmakerId");
            AddForeignKey("dbo.Fixtures", "BestHomeOdds_BookmakerId", "dbo.Bookmakers", "BookmakerId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Bookmakers", "Fixture_FixtureId", "dbo.Fixtures");
            DropForeignKey("dbo.Fixtures", "BestHomeOdds_BookmakerId", "dbo.Bookmakers");
            DropForeignKey("dbo.Fixtures", "BestDrawOdds_BookmakerId", "dbo.Bookmakers");
            DropForeignKey("dbo.Fixtures", "BestAwayOdds_BookmakerId", "dbo.Bookmakers");
            DropIndex("dbo.Bookmakers", new[] { "Fixture_FixtureId" });
            DropIndex("dbo.Fixtures", new[] { "BestHomeOdds_BookmakerId" });
            DropIndex("dbo.Fixtures", new[] { "BestDrawOdds_BookmakerId" });
            DropIndex("dbo.Fixtures", new[] { "BestAwayOdds_BookmakerId" });
            DropColumn("dbo.Fixtures", "BestHomeOdds_BookmakerId");
            DropColumn("dbo.Fixtures", "BestDrawOdds_BookmakerId");
            DropColumn("dbo.Fixtures", "BestAwayOdds_BookmakerId");
            DropTable("dbo.Bookmakers");
        }
    }
}
