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
    /// <summary>Provides data access operations for <see cref="Librarian"/> entities.</summary>
    /// <remarks>
    /// Implements <see cref="ILibrarianRepository"/> to provide CRUD operations and librarian-specific queries.
    /// Manages librarian accounts and their one-to-one relationships with reader profiles.
    /// </remarks>
    public class LibrarianRepository : ILibrarianRepository
    {
        /// <summary>The Entity Framework database context for accessing librarian data.</summary>
        private readonly LibraryContext _context;

        /// <summary>Initializes a new instance of the <see cref="LibrarianRepository"/> class.</summary>
        /// <param name="context">The database context for accessing librarian data.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        public LibrarianRepository(LibraryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<Librarian> GetByIdAsync(Guid id)
        {
            return await _context.Librarians
                .Include(l => l.ReaderDetails)
                .Include(l => l.ProcessedLoans)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Librarian>> GetAllAsync()
        {
            return await _context.Librarians
                .Include(l => l.ReaderDetails)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Librarians.AnyAsync(l => l.Id == id);
        }

        /// <inheritdoc/>
        public async Task<Librarian> AddAsync(Librarian entity)
        {
            return await Task.FromResult(_context.Librarians.Add(entity));
        }

        /// <inheritdoc/>
        public async Task<Librarian> UpdateAsync(Librarian entity)
        {
            var existingEntity = await _context.Librarians.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            return existingEntity;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var librarian = await _context.Librarians.FindAsync(id);
            if (librarian == null)
                return false;

            _context.Librarians.Remove(librarian);
            return true;
        }

        /// <inheritdoc/>
        public async Task<Librarian> GetByReaderIdAsync(Guid readerId)
        {
            return await _context.Librarians
                .Include(l => l.ReaderDetails)
                .FirstOrDefaultAsync(l => l.ReaderDetails != null && l.ReaderDetails.Id == readerId);
        }

        /// <inheritdoc/>
        public async Task<bool> IsReaderAlsoLibrarianAsync(Guid readerId)
        {
            return await _context.Librarians
                .AnyAsync(l => l.ReaderDetails != null && l.ReaderDetails.Id == readerId);
        }
    }
}
