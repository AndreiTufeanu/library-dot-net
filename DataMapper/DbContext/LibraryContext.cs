using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainModel;

namespace DataMapper
{
    public class LibraryContext : DbContext
    {
        public LibraryContext() : base("name=LibraryDBConnectionString")
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Librarian>()
                .HasOptional(l => l.ReaderDetails)
                .WithOptionalDependent(r => r.LibrarianAccount);
        }

        /// <summary>Gets or sets the books.</summary>
        /// <value>The books.</value>
        public DbSet<Book> Books { get; set; }

        /// <summary>Gets or sets the book types.</summary>
        /// <value>The book types.</value>
        public DbSet<BookType> BookTypes { get; set; }

        /// <summary>Gets or sets the authors.</summary>
        /// <value>The authors.</value>
        public DbSet<Author> Authors { get; set; }

        /// <summary>Gets or sets the domains.</summary>
        /// <value>The domains.</value>
        public DbSet<Domain> Domains { get; set; }

        /// <summary>Gets or sets the editions.</summary>
        /// <value>The editions.</value>
        public DbSet<Edition> Editions { get; set; }

        /// <summary>Gets or sets the book copies.</summary>
        /// <value>The book copies.</value>
        public DbSet<BookCopy> BookCopies { get; set; }

        /// <summary>Gets or sets the readers.</summary>
        /// <value>The readers.</value>
        public DbSet<Reader> Readers { get; set; }

        /// <summary>Gets or sets the librarians.</summary>
        /// <value>The librarians.</value>
        public DbSet<Librarian> Librarians { get; set; }

        /// <summary>Gets or sets the borrowings.</summary>
        /// <value>The borrowings.</value>
        public DbSet<Borrowing> Borrowings { get; set; }
    }
}
