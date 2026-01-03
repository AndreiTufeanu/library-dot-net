namespace Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddValidationToAllEntities : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Editions", "Book_Id", "dbo.Books");
            DropForeignKey("dbo.BookCopies", "Edition_Id", "dbo.Editions");
            DropForeignKey("dbo.Editions", "BookType_Id", "dbo.BookTypes");
            DropForeignKey("dbo.Borrowings", "Librarian_Id", "dbo.Librarians");
            DropForeignKey("dbo.Borrowings", "Reader_Id", "dbo.Readers");
            DropIndex("dbo.Editions", new[] { "Book_Id" });
            DropIndex("dbo.Editions", new[] { "BookType_Id" });
            DropIndex("dbo.BookCopies", new[] { "Edition_Id" });
            DropIndex("dbo.Borrowings", new[] { "Librarian_Id" });
            DropIndex("dbo.Borrowings", new[] { "Reader_Id" });
            AlterColumn("dbo.Books", "Title", c => c.String(nullable: false, maxLength: 300));
            AlterColumn("dbo.Editions", "Book_Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.Editions", "BookType_Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.BookCopies", "Edition_Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.Borrowings", "Librarian_Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.Borrowings", "Reader_Id", c => c.Guid(nullable: false));
            CreateIndex("dbo.Editions", "Book_Id");
            CreateIndex("dbo.Editions", "BookType_Id");
            CreateIndex("dbo.BookCopies", "Edition_Id");
            CreateIndex("dbo.Borrowings", "Librarian_Id");
            CreateIndex("dbo.Borrowings", "Reader_Id");
            AddForeignKey("dbo.Editions", "Book_Id", "dbo.Books", "Id", cascadeDelete: true);
            AddForeignKey("dbo.BookCopies", "Edition_Id", "dbo.Editions", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Editions", "BookType_Id", "dbo.BookTypes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Borrowings", "Librarian_Id", "dbo.Librarians", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Borrowings", "Reader_Id", "dbo.Readers", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Borrowings", "Reader_Id", "dbo.Readers");
            DropForeignKey("dbo.Borrowings", "Librarian_Id", "dbo.Librarians");
            DropForeignKey("dbo.Editions", "BookType_Id", "dbo.BookTypes");
            DropForeignKey("dbo.BookCopies", "Edition_Id", "dbo.Editions");
            DropForeignKey("dbo.Editions", "Book_Id", "dbo.Books");
            DropIndex("dbo.Borrowings", new[] { "Reader_Id" });
            DropIndex("dbo.Borrowings", new[] { "Librarian_Id" });
            DropIndex("dbo.BookCopies", new[] { "Edition_Id" });
            DropIndex("dbo.Editions", new[] { "BookType_Id" });
            DropIndex("dbo.Editions", new[] { "Book_Id" });
            AlterColumn("dbo.Borrowings", "Reader_Id", c => c.Guid());
            AlterColumn("dbo.Borrowings", "Librarian_Id", c => c.Guid());
            AlterColumn("dbo.BookCopies", "Edition_Id", c => c.Guid());
            AlterColumn("dbo.Editions", "BookType_Id", c => c.Guid());
            AlterColumn("dbo.Editions", "Book_Id", c => c.Guid());
            AlterColumn("dbo.Books", "Title", c => c.String(maxLength: 300));
            CreateIndex("dbo.Borrowings", "Reader_Id");
            CreateIndex("dbo.Borrowings", "Librarian_Id");
            CreateIndex("dbo.BookCopies", "Edition_Id");
            CreateIndex("dbo.Editions", "BookType_Id");
            CreateIndex("dbo.Editions", "Book_Id");
            AddForeignKey("dbo.Borrowings", "Reader_Id", "dbo.Readers", "Id");
            AddForeignKey("dbo.Borrowings", "Librarian_Id", "dbo.Librarians", "Id");
            AddForeignKey("dbo.Editions", "BookType_Id", "dbo.BookTypes", "Id");
            AddForeignKey("dbo.BookCopies", "Edition_Id", "dbo.Editions", "Id");
            AddForeignKey("dbo.Editions", "Book_Id", "dbo.Books", "Id");
        }
    }
}
