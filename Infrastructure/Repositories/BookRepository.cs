using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using Infrastructure.ApplicationContext;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    /// <summary>Provides data access operations for <see cref="Book"/> entities.</summary>
    /// <remarks>
    /// Implements <see cref="IBookRepository"/> to provide CRUD operations and book-specific queries.
    /// Manages book data including relationships with authors, domains, editions, and physical copies.
    /// </remarks>
    public class BookRepository : IBookRepository
    {
        /// <summary>The Entity Framework database context for accessing book data.</summary>
        private readonly LibraryContext _context;

        /// <summary>Initializes a new instance of the <see cref="BookRepository"/> class.</summary>
        /// <param name="context">The database context for accessing book data.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        public BookRepository(LibraryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<Book> GetByIdAsync(Guid id)
        {
            return await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.Domains)
                .Include(b => b.Editions.Select(e => e.BookCopies))
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.Domains)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Books.AnyAsync(b => b.Id == id);
        }

        /// <inheritdoc/>
        public async Task<Book> AddAsync(Book entity)
        {
            return await Task.FromResult(_context.Books.Add(entity));
        }

        /// <inheritdoc/>
        public async Task<Book> UpdateAsync(Book entity)
        {
            var existingEntity = await _context.Books.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            return existingEntity;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return false;

            _context.Books.Remove(book);
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> HasPhysicalCopiesAsync(Guid id)
        {
            return await _context.Books
                .Where(b => b.Id == id)
                .SelectMany(b => b.Editions)
                .SelectMany(e => e.BookCopies)
                .AnyAsync();
        }
    }
}
