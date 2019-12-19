namespace BettingPredictorV3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLeagueForeignKey : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Teams", "League_LeagueID", "dbo.Leagues");
            DropIndex("dbo.Teams", new[] { "League_LeagueID" });
            DropPrimaryKey("dbo.Leagues");
            AddColumn("dbo.Fixtures", "LeagueId", c => c.Int(nullable: false));
            AddColumn("dbo.Fixtures", "HomeGoals", c => c.Double(nullable: false));
            AddColumn("dbo.Fixtures", "AwayGoals", c => c.Double(nullable: false));
            AddColumn("dbo.Leagues", "LeagueCode", c => c.String());
            AlterColumn("dbo.Teams", "League_LeagueId", c => c.Int());
            AlterColumn("dbo.Leagues", "LeagueId", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.Leagues", "LeagueId");
            CreateIndex("dbo.Fixtures", "LeagueId");
            CreateIndex("dbo.Teams", "League_LeagueId");
            AddForeignKey("dbo.Fixtures", "LeagueId", "dbo.Leagues", "LeagueId", cascadeDelete: true);
            AddForeignKey("dbo.Teams", "League_LeagueId", "dbo.Leagues", "LeagueId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Teams", "League_LeagueId", "dbo.Leagues");
            DropForeignKey("dbo.Fixtures", "LeagueId", "dbo.Leagues");
            DropIndex("dbo.Teams", new[] { "League_LeagueId" });
            DropIndex("dbo.Fixtures", new[] { "LeagueId" });
            DropPrimaryKey("dbo.Leagues");
            AlterColumn("dbo.Leagues", "LeagueId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.Teams", "League_LeagueId", c => c.String(maxLength: 128));
            DropColumn("dbo.Leagues", "LeagueCode");
            DropColumn("dbo.Fixtures", "AwayGoals");
            DropColumn("dbo.Fixtures", "HomeGoals");
            DropColumn("dbo.Fixtures", "LeagueId");
            AddPrimaryKey("dbo.Leagues", "LeagueID");
            CreateIndex("dbo.Teams", "League_LeagueID");
            AddForeignKey("dbo.Teams", "League_LeagueID", "dbo.Leagues", "LeagueID");
        }
    }
}
