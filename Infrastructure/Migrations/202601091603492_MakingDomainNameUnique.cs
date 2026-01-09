namespace Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MakingDomainNameUnique : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Domains", "Name", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.Domains", new[] { "Name" });
        }
    }
}
