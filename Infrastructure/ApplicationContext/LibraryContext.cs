using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainModel;
using DomainModel.Entities;

namespace Infrastructure.ApplicationContext
{
    /// <summary>Represents the database context for the library management system.</summary>
    public class LibraryContext : DbContext
    {
        /// <summary>Initializes a new instance of the <see cref="LibraryContext"/> class.</summary>
        /// <remarks>Uses the connection string named "LibraryDBConnectionString" from the configuration file.</remarks>
        public LibraryContext() : base("name=LibraryDBConnectionString")
        {

        }

        /// <summary>Configures the model that was discovered by convention from the entity types.</summary>
        /// <param name="modelBuilder">The builder that defines the model for the context being created.</param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Librarian>()
                .HasOptional(l => l.ReaderDetails)
                .WithOptionalDependent(r => r.LibrarianAccount);
        }

        /// <summary>Gets or sets the collection of books in the library.</summary>
        /// <value>A <see cref="DbSet{Book}"/> representing the books table in the database.</value>
        public DbSet<Book> Books { get; set; }

        /// <summary>Gets or sets the collection of book types in the library.</summary>
        /// <value>A <see cref="DbSet{BookType}"/> representing the book types table in the database (e.g., Hardcover, Paperback).</value>
        public DbSet<BookType> BookTypes { get; set; }

        /// <summary>Gets or sets the collection of authors in the library.</summary>
        /// <value>A <see cref="DbSet{Author}"/> representing the authors table in the database.</value>
        public DbSet<Author> Authors { get; set; }

        /// <summary>Gets or sets the collection of book domains/categories in the library.</summary>
        /// <value>A <see cref="DbSet{Domain}"/> representing the domains table in the database, used for categorizing books hierarchically.</value>
        public DbSet<Domain> Domains { get; set; }

        /// <summary>Gets or sets the collection of book editions in the library.</summary>
        /// <value>A <see cref="DbSet{Edition}"/> representing the editions table in the database, containing different versions of books.</value>
        public DbSet<Edition> Editions { get; set; }

        /// <summary>Gets or sets the collection of physical book copies in the library.</summary>
        /// <value>A <see cref="DbSet{BookCopy}"/> representing the book copies table in the database, tracking individual physical copies.</value>
        public DbSet<BookCopy> BookCopies { get; set; }

        /// <summary>Gets or sets the collection of library readers/patrons.</summary>
        /// <value>A <see cref="DbSet{Reader}"/> representing the readers table in the database, containing library member information.</value>
        public DbSet<Reader> Readers { get; set; }

        /// <summary>Gets or sets the collection of librarians in the library.</summary>
        /// <value>A <see cref="DbSet{Librarian}"/> representing the librarians table in the database, containing staff information.</value>
        public DbSet<Librarian> Librarians { get; set; }

        /// <summary>Gets or sets the collection of book borrowing transactions.</summary>
        /// <value>A <see cref="DbSet{Borrowing}"/> representing the borrowings table in the database, tracking book loan history.</value>
        public DbSet<Borrowing> Borrowings { get; set; }

        /// <summary>Gets or sets the collection of configuration settings for the library system.</summary>
        /// <value>A <see cref="DbSet{ConfigurationSetting}"/> representing the configuration settings table in the database.</value>
        public DbSet<ConfigurationSetting> ConfigurationSettings { get; set; }
    }
}