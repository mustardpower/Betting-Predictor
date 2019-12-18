namespace BettingPredictorV3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Fixtures",
                c => new
                    {
                        FixtureId = c.Int(nullable: false, identity: true),
                        HomeForm = c.Int(nullable: false),
                        AwayForm = c.Int(nullable: false),
                        HomeResidual = c.Double(nullable: false),
                        AwayResidual = c.Double(nullable: false),
                        PredictedGoalDifference = c.Double(nullable: false),
                        AverageHomeResidual = c.Double(nullable: false),
                        AverageAwayResidual = c.Double(nullable: false),
                        Arbitrage = c.Double(nullable: false),
                        KellyCriterionHome = c.Double(nullable: false),
                        KellyCriterionDraw = c.Double(nullable: false),
                        KellyCriterionAway = c.Double(nullable: false),
                        BestAwayOdds_BookmakerId = c.Int(),
                        BestDrawOdds_BookmakerId = c.Int(),
                        BestHomeOdds_BookmakerId = c.Int(),
                        Team_TeamId = c.Int(),
                    })
                .PrimaryKey(t => t.FixtureId)
                .ForeignKey("dbo.Bookmakers", t => t.BestAwayOdds_BookmakerId)
                .ForeignKey("dbo.Bookmakers", t => t.BestDrawOdds_BookmakerId)
                .ForeignKey("dbo.Bookmakers", t => t.BestHomeOdds_BookmakerId)
                .ForeignKey("dbo.Teams", t => t.Team_TeamId)
                .Index(t => t.BestAwayOdds_BookmakerId)
                .Index(t => t.BestDrawOdds_BookmakerId)
                .Index(t => t.BestHomeOdds_BookmakerId)
                .Index(t => t.Team_TeamId);
            
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
            
            CreateTable(
                "dbo.Leagues",
                c => new
                    {
                        LeagueID = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.LeagueID);
            
            CreateTable(
                "dbo.Teams",
                c => new
                    {
                        TeamId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Form = c.Int(nullable: false),
                        League_LeagueID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.TeamId)
                .ForeignKey("dbo.Leagues", t => t.League_LeagueID)
                .Index(t => t.League_LeagueID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Teams", "League_LeagueID", "dbo.Leagues");
            DropForeignKey("dbo.Fixtures", "Team_TeamId", "dbo.Teams");
            DropForeignKey("dbo.Bookmakers", "Fixture_FixtureId", "dbo.Fixtures");
            DropForeignKey("dbo.Fixtures", "BestHomeOdds_BookmakerId", "dbo.Bookmakers");
            DropForeignKey("dbo.Fixtures", "BestDrawOdds_BookmakerId", "dbo.Bookmakers");
            DropForeignKey("dbo.Fixtures", "BestAwayOdds_BookmakerId", "dbo.Bookmakers");
            DropIndex("dbo.Teams", new[] { "League_LeagueID" });
            DropIndex("dbo.Bookmakers", new[] { "Fixture_FixtureId" });
            DropIndex("dbo.Fixtures", new[] { "Team_TeamId" });
            DropIndex("dbo.Fixtures", new[] { "BestHomeOdds_BookmakerId" });
            DropIndex("dbo.Fixtures", new[] { "BestDrawOdds_BookmakerId" });
            DropIndex("dbo.Fixtures", new[] { "BestAwayOdds_BookmakerId" });
            DropTable("dbo.Teams");
            DropTable("dbo.Leagues");
            DropTable("dbo.Bookmakers");
            DropTable("dbo.Fixtures");
        }
    }
}
