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
    /// <summary>Provides data access operations for <see cref="Edition"/> entities.</summary>
    /// <remarks>
    /// Implements <see cref="IEditionRepository"/> to provide CRUD operations and edition-specific queries.
    /// Manages book edition data including relationships with books, book types, and physical copies.
    /// </remarks>
    public class EditionRepository : IEditionRepository
    {
        /// <summary>The Entity Framework database context for accessing edition data.</summary>
        private readonly LibraryContext _context;

        /// <summary>Initializes a new instance of the <see cref="EditionRepository"/> class.</summary>
        /// <param name="context">The database context for accessing edition data.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        public EditionRepository(LibraryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>

        public async Task<Edition> GetByIdAsync(Guid id)
        {
            return await _context.Editions
                .Include(e => e.Book)
                .Include(e => e.BookType)
                .Include(e => e.BookCopies)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Edition>> GetAllAsync()
        {
            return await _context.Editions
                .Include(e => e.Book)
                .Include(e => e.BookType)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Editions.AnyAsync(e => e.Id == id);
        }

        /// <inheritdoc/>
        public async Task<Edition> AddAsync(Edition entity)
        {
            return await Task.FromResult(_context.Editions.Add(entity));
        }

        /// <inheritdoc/>
        public async Task<Edition> UpdateAsync(Edition entity)
        {
            var existingEntity = await _context.Editions.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            return existingEntity;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var edition = await _context.Editions.FindAsync(id);
            if (edition == null)
                return false;

            _context.Editions.Remove(edition);
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> HasCopiesAsync(Guid id)
        {
            return await _context.Editions
                .Where(e => e.Id == id)
                .SelectMany(e => e.BookCopies)
                .AnyAsync();
        }
    }
}
