namespace DataMapper.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Authors",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        FirstName = c.String(nullable: false, maxLength: 100),
                        LastName = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Books",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Title = c.String(maxLength: 300),
                        Description = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Domains",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 1000),
                        ParentDomain_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Domains", t => t.ParentDomain_Id)
                .Index(t => t.ParentDomain_Id);
            
            CreateTable(
                "dbo.Editions",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        NumberOfPages = c.Int(nullable: false),
                        PublicationDate = c.DateTime(nullable: false),
                        Book_Id = c.Guid(),
                        BookType_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Books", t => t.Book_Id)
                .ForeignKey("dbo.BookTypes", t => t.BookType_Id)
                .Index(t => t.Book_Id)
                .Index(t => t.BookType_Id);
            
            CreateTable(
                "dbo.BookCopies",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        IsLectureRoomOnly = c.Boolean(nullable: false),
                        IsAvailable = c.Boolean(nullable: false),
                        Edition_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Editions", t => t.Edition_Id)
                .Index(t => t.Edition_Id);
            
            CreateTable(
                "dbo.Borrowings",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BorrowDate = c.DateTime(nullable: false),
                        DueDate = c.DateTime(nullable: false),
                        ReturnDate = c.DateTime(),
                        ExtendedDueDate = c.DateTime(),
                        ExtensionDays = c.Int(),
                        Librarian_Id = c.Guid(),
                        Reader_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Librarians", t => t.Librarian_Id)
                .ForeignKey("dbo.Readers", t => t.Reader_Id)
                .Index(t => t.Librarian_Id)
                .Index(t => t.Reader_Id);
            
            CreateTable(
                "dbo.Librarians",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ReaderDetails_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Readers", t => t.ReaderDetails_Id)
                .Index(t => t.ReaderDetails_Id);
            
            CreateTable(
                "dbo.Readers",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        FirstName = c.String(nullable: false, maxLength: 100),
                        LastName = c.String(nullable: false, maxLength: 100),
                        Address = c.String(nullable: false, maxLength: 300),
                        DateOfBirth = c.DateTime(nullable: false),
                        PhoneNumber = c.String(maxLength: 20),
                        Email = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BookTypes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BookAuthors",
                c => new
                    {
                        Book_Id = c.Guid(nullable: false),
                        Author_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Book_Id, t.Author_Id })
                .ForeignKey("dbo.Books", t => t.Book_Id, cascadeDelete: true)
                .ForeignKey("dbo.Authors", t => t.Author_Id, cascadeDelete: true)
                .Index(t => t.Book_Id)
                .Index(t => t.Author_Id);
            
            CreateTable(
                "dbo.DomainBooks",
                c => new
                    {
                        Domain_Id = c.Guid(nullable: false),
                        Book_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Domain_Id, t.Book_Id })
                .ForeignKey("dbo.Domains", t => t.Domain_Id, cascadeDelete: true)
                .ForeignKey("dbo.Books", t => t.Book_Id, cascadeDelete: true)
                .Index(t => t.Domain_Id)
                .Index(t => t.Book_Id);
            
            CreateTable(
                "dbo.BorrowingBookCopies",
                c => new
                    {
                        Borrowing_Id = c.Guid(nullable: false),
                        BookCopy_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Borrowing_Id, t.BookCopy_Id })
                .ForeignKey("dbo.Borrowings", t => t.Borrowing_Id, cascadeDelete: true)
                .ForeignKey("dbo.BookCopies", t => t.BookCopy_Id, cascadeDelete: true)
                .Index(t => t.Borrowing_Id)
                .Index(t => t.BookCopy_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Editions", "BookType_Id", "dbo.BookTypes");
            DropForeignKey("dbo.BookCopies", "Edition_Id", "dbo.Editions");
            DropForeignKey("dbo.Librarians", "ReaderDetails_Id", "dbo.Readers");
            DropForeignKey("dbo.Borrowings", "Reader_Id", "dbo.Readers");
            DropForeignKey("dbo.Borrowings", "Librarian_Id", "dbo.Librarians");
            DropForeignKey("dbo.BorrowingBookCopies", "BookCopy_Id", "dbo.BookCopies");
            DropForeignKey("dbo.BorrowingBookCopies", "Borrowing_Id", "dbo.Borrowings");
            DropForeignKey("dbo.Editions", "Book_Id", "dbo.Books");
            DropForeignKey("dbo.Domains", "ParentDomain_Id", "dbo.Domains");
            DropForeignKey("dbo.DomainBooks", "Book_Id", "dbo.Books");
            DropForeignKey("dbo.DomainBooks", "Domain_Id", "dbo.Domains");
            DropForeignKey("dbo.BookAuthors", "Author_Id", "dbo.Authors");
            DropForeignKey("dbo.BookAuthors", "Book_Id", "dbo.Books");
            DropIndex("dbo.BorrowingBookCopies", new[] { "BookCopy_Id" });
            DropIndex("dbo.BorrowingBookCopies", new[] { "Borrowing_Id" });
            DropIndex("dbo.DomainBooks", new[] { "Book_Id" });
            DropIndex("dbo.DomainBooks", new[] { "Domain_Id" });
            DropIndex("dbo.BookAuthors", new[] { "Author_Id" });
            DropIndex("dbo.BookAuthors", new[] { "Book_Id" });
            DropIndex("dbo.Librarians", new[] { "ReaderDetails_Id" });
            DropIndex("dbo.Borrowings", new[] { "Reader_Id" });
            DropIndex("dbo.Borrowings", new[] { "Librarian_Id" });
            DropIndex("dbo.BookCopies", new[] { "Edition_Id" });
            DropIndex("dbo.Editions", new[] { "BookType_Id" });
            DropIndex("dbo.Editions", new[] { "Book_Id" });
            DropIndex("dbo.Domains", new[] { "ParentDomain_Id" });
            DropTable("dbo.BorrowingBookCopies");
            DropTable("dbo.DomainBooks");
            DropTable("dbo.BookAuthors");
            DropTable("dbo.BookTypes");
            DropTable("dbo.Readers");
            DropTable("dbo.Librarians");
            DropTable("dbo.Borrowings");
            DropTable("dbo.BookCopies");
            DropTable("dbo.Editions");
            DropTable("dbo.Domains");
            DropTable("dbo.Books");
            DropTable("dbo.Authors");
        }
    }
}
