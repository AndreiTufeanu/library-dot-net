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
    /// <summary>Provides data access operations for <see cref="Domain"/> entities.</summary>
    /// <remarks>
    /// Implements <see cref="IDomainRepository"/> to provide CRUD operations and hierarchical domain queries.
    /// Manages the domain tree structure with parent-child relationships and book categorization.
    /// </remarks>
    public class DomainRepository : IDomainRepository
    {
        /// <summary>The Entity Framework database context for accessing domain data.</summary>
        private readonly LibraryContext _context;

        /// <summary>Initializes a new instance of the <see cref="DomainRepository"/> class.</summary>
        /// <param name="context">The database context for accessing domain data.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        public DomainRepository(LibraryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<Domain> GetByIdAsync(Guid id)
        {
            return await _context.Domains
                .Include(d => d.ParentDomain)
                .Include(d => d.Subdomains)
                .Include(d => d.Books)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Domain>> GetAllAsync()
        {
            return await _context.Domains
                .Include(d => d.ParentDomain)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Domains.AnyAsync(d => d.Id == id);
        }

        /// <inheritdoc/>
        public async Task<Domain> AddAsync(Domain entity)
        {
            return await Task.FromResult(_context.Domains.Add(entity));
        }

        /// <inheritdoc/>
        public async Task<Domain> UpdateAsync(Domain entity)
        {
            var existingEntity = await _context.Domains.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            return existingEntity;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var domain = await _context.Domains.FindAsync(id);
            if (domain == null)
                return false;

            _context.Domains.Remove(domain);
            return true;
        }

        /// <inheritdoc/>
        public async Task<Domain> FindByNameAsync(string name)
        {
            return await _context.Domains
                .Include(d => d.ParentDomain)
                .Include(d => d.Subdomains)
                .FirstOrDefaultAsync(d => d.Name.ToLower() == name.ToLower());
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Domain>> GetSubdomainsAsync(Guid parentDomainId)
        {
            return await _context.Domains
                .Include(d => d.Subdomains)
                .Where(d => d.ParentDomain != null && d.ParentDomain.Id == parentDomainId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> HasBooksAsync(Guid id)
        {
            return await _context.Domains
                .Where(d => d.Id == id)
                .SelectMany(d => d.Books)
                .AnyAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> HasSubdomainsAsync(Guid id)
        {
            return await _context.Domains
                .AnyAsync(d => d.ParentDomain != null && d.ParentDomain.Id == id);
        }
    }
}
