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
    /// <summary>Provides data access operations for <see cref="Author"/> entities.</summary>
    /// <remarks>
    /// Implements <see cref="IAuthorRepository"/> to provide CRUD operations and author-specific queries.
    /// Uses Entity Framework for database access with eager loading of related entities.
    /// </remarks>
    public class AuthorRepository : IAuthorRepository
    {
        /// <summary>The Entity Framework database context for accessing author data.</summary>
        private readonly LibraryContext _context;

        /// <summary>Initializes a new instance of the <see cref="AuthorRepository"/> class.</summary>
        /// <param name="context">The database context for accessing author data.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        public AuthorRepository(LibraryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<Author> GetByIdAsync(Guid id)
        {
            return await _context.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Author>> GetAllAsync()
        {
            return await _context.Authors
                .Include(a => a.Books)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Authors.AnyAsync(a => a.Id == id);
        }

        /// <inheritdoc/>
        public async Task<Author> AddAsync(Author entity)
        {
            return await Task.FromResult(_context.Authors.Add(entity));
        }

        /// <inheritdoc/>
        public async Task<Author> UpdateAsync(Author entity)
        {
            var existingEntity = await _context.Authors.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            return existingEntity;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null)
                return false;

            _context.Authors.Remove(author);
            return true;
        }

        /// <inheritdoc/>
        public async Task<Author> FindByNameAsync(string firstName, string lastName)
        {
            return await _context.Authors
                .FirstOrDefaultAsync(a =>
                    a.FirstName.ToLower() == firstName.ToLower() &&
                    a.LastName.ToLower() == lastName.ToLower());
        }

        /// <inheritdoc/>
        public async Task<bool> HasBooksAsync(Guid id)
        {
            return await _context.Authors
                .Where(a => a.Id == id)
                .SelectMany(a => a.Books)
                .AnyAsync();
        }
    }
}
