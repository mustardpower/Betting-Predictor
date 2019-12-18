namespace BettingPredictorV3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixfixturesforeignkeys : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Fixtures", "HomeTeamId", c => c.Int(nullable: false));
            AddColumn("dbo.Fixtures", "AwayTeamId", c => c.Int(nullable: false));
            CreateIndex("dbo.Fixtures", "HomeTeamId");
            CreateIndex("dbo.Fixtures", "AwayTeamId");
            AddForeignKey("dbo.Fixtures", "AwayTeamId", "dbo.Teams", "TeamId");
            AddForeignKey("dbo.Fixtures", "HomeTeamId", "dbo.Teams", "TeamId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Fixtures", "HomeTeamId", "dbo.Teams");
            DropForeignKey("dbo.Fixtures", "AwayTeamId", "dbo.Teams");
            DropIndex("dbo.Fixtures", new[] { "AwayTeamId" });
            DropIndex("dbo.Fixtures", new[] { "HomeTeamId" });
            DropColumn("dbo.Fixtures", "AwayTeamId");
            DropColumn("dbo.Fixtures", "HomeTeamId");
        }
    }
}
