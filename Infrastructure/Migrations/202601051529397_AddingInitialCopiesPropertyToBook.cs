namespace Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingInitialCopiesPropertyToBook : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Books", "InitialCopies", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Books", "InitialCopies");
        }
    }
}
