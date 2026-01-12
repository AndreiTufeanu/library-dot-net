using DomainModel.Entities;
using Infrastructure.ApplicationContext;
using DomainModel.RepositoryContracts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    /// <summary>Provides data access operations for <see cref="BookType"/> entities.</summary>
    /// <remarks>
    /// Implements <see cref="IBookTypeRepository"/> to provide CRUD operations and book type-specific queries.
    /// Manages book type reference data (e.g., Hardcover, Paperback) and their relationships with editions.
    /// </remarks>
    public class BookTypeRepository : IBookTypeRepository
    {
        /// <summary>The Entity Framework database context for accessing book type data.</summary>
        private readonly LibraryContext _context;

        /// <summary>Initializes a new instance of the <see cref="BookTypeRepository"/> class.</summary>
        /// <param name="context">The database context for accessing book type data.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        public BookTypeRepository(LibraryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<BookType> GetByIdAsync(Guid id)
        {
            return await _context.BookTypes
                .Include(bt => bt.Editions)
                .FirstOrDefaultAsync(bt => bt.Id == id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BookType>> GetAllAsync()
        {
            return await _context.BookTypes
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.BookTypes.AnyAsync(bt => bt.Id == id);
        }

        /// <inheritdoc/>
        public async Task<BookType> FindByNameAsync(string name)
        {
            return await _context.BookTypes
                .FirstOrDefaultAsync(bt => bt.Name.ToLower() == name.ToLower());
        }

        /// <inheritdoc/>
        public async Task<BookType> AddAsync(BookType entity)
        {
            var addedEntity = _context.BookTypes.Add(entity);
            return await Task.FromResult(addedEntity);
        }

        /// <inheritdoc/>
        public async Task<BookType> UpdateAsync(BookType entity)
        {
            var existingEntity = await _context.BookTypes.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            return existingEntity;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var bookType = await _context.BookTypes.FindAsync(id);
            if (bookType == null)
                return false;

            _context.BookTypes.Remove(bookType);
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> HasEditionsAsync(Guid id)
        {
            return await _context.BookTypes
                .Where(bt => bt.Id == id)
                .SelectMany(bt => bt.Editions)
                .AnyAsync();
        }
    }
}
