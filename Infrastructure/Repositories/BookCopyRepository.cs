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
    /// <summary>Provides data access operations for <see cref="BookCopy"/> entities.</summary>
    /// <remarks>
    /// Implements <see cref="IBookCopyRepository"/> to provide CRUD operations and book copy-specific queries.
    /// Manages the state and availability of physical book copies in the library.
    /// </remarks>
    public class BookCopyRepository : IBookCopyRepository
    {
        /// <summary>The Entity Framework database context for accessing book copy data.</summary>
        private readonly LibraryContext _context;

        /// <summary>Initializes a new instance of the <see cref="BookCopyRepository"/> class.</summary>
        /// <param name="context">The database context for accessing book copy data.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        public BookCopyRepository(LibraryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<BookCopy> GetByIdAsync(Guid id)
        {
            return await _context.BookCopies
                .Include(bc => bc.Edition.Book)
                .Include(bc => bc.Borrowings)
                .FirstOrDefaultAsync(bc => bc.Id == id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BookCopy>> GetAllAsync()
        {
            return await _context.BookCopies
                .Include(bc => bc.Edition.Book)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.BookCopies.AnyAsync(bc => bc.Id == id);
        }

        /// <inheritdoc/>
        public async Task<BookCopy> AddAsync(BookCopy entity)
        {
            return await Task.FromResult(_context.BookCopies.Add(entity));
        }

        /// <inheritdoc/>
        public async Task<BookCopy> UpdateAsync(BookCopy entity)
        {
            var existingEntity = await _context.BookCopies.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            return existingEntity;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var bookCopy = await _context.BookCopies.FindAsync(id);
            if (bookCopy == null)
                return false;

            _context.BookCopies.Remove(bookCopy);
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> IsCurrentlyBorrowedAsync(Guid bookCopyId)
        {
            return await _context.Borrowings
                .AnyAsync(b => b.BookCopies.Any(bc => bc.Id == bookCopyId) && b.ReturnDate == null);
        }
    }
}
