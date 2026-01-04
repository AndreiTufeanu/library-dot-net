namespace Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingConfigurationSetting : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ConfigurationSettings",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Key = c.String(nullable: false, maxLength: 100),
                        Value = c.String(nullable: false),
                        Description = c.String(maxLength: 500),
                        DataType = c.String(nullable: false, maxLength: 50),
                        Category = c.String(maxLength: 100),
                        LastModified = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Key, unique: true);
            
            DropColumn("dbo.Borrowings", "ExtendedDueDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Borrowings", "ExtendedDueDate", c => c.DateTime());
            DropIndex("dbo.ConfigurationSettings", new[] { "Key" });
            DropTable("dbo.ConfigurationSettings");
        }
    }
}
