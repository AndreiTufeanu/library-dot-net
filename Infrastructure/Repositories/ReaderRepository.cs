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
    /// <summary>Provides data access operations for <see cref="Reader"/> entities.</summary>
    /// <remarks>
    /// Implements <see cref="IReaderRepository"/> to provide CRUD operations and reader-specific queries.
    /// Manages library patron data including contact information, borrowing history, and librarian relationships.
    /// </remarks>
    public class ReaderRepository : IReaderRepository
    {
        /// <summary>The Entity Framework database context for accessing reader data.</summary>
        private readonly LibraryContext _context;

        /// <summary>Initializes a new instance of the <see cref="ReaderRepository"/> class.</summary>
        /// <param name="context">The database context for accessing reader data.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        public ReaderRepository(LibraryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<Reader> GetByIdAsync(Guid id)
        {
            return await _context.Readers
                .Include(r => r.Borrowings)
                .Include(r => r.LibrarianAccount)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Reader>> GetAllAsync()
        {
            return await _context.Readers
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Readers.AnyAsync(r => r.Id == id);
        }

        /// <inheritdoc/>
        public async Task<Reader> AddAsync(Reader entity)
        {
            return await Task.FromResult(_context.Readers.Add(entity));
        }

        /// <inheritdoc/>
        public async Task<Reader> UpdateAsync(Reader entity)
        {
            var existingEntity = await _context.Readers.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            return existingEntity;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var reader = await _context.Readers.FindAsync(id);
            if (reader == null)
                return false;

            _context.Readers.Remove(reader);
            return true;
        }

        /// <inheritdoc/>
        public async Task<Reader> FindByEmailAsync(string email)
        {
            return await _context.Readers
                .FirstOrDefaultAsync(r => r.Email.ToLower() == email.ToLower());
        }

        /// <inheritdoc/>
        public async Task<Reader> FindByPhoneAsync(string phoneNumber)
        {
            return await _context.Readers
                .FirstOrDefaultAsync(r => r.PhoneNumber == phoneNumber);
        }

        /// <inheritdoc/>
        public async Task<bool> HasActiveBorrowingsAsync(Guid id)
        {
            return await _context.Borrowings
                .AnyAsync(b => b.Reader.Id == id && b.ReturnDate == null);
        }
    }
}
