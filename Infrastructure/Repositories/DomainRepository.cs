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
    public class DomainRepository : IDomainRepository
    {
        private readonly LibraryContext _context;

        public DomainRepository(LibraryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Domain> GetByIdAsync(Guid id)
        {
            return await _context.Domains
                .Include(d => d.ParentDomain)
                .Include(d => d.Subdomains)
                .Include(d => d.Books)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Domain>> GetAllAsync()
        {
            return await _context.Domains
                .Include(d => d.ParentDomain)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Domains.AnyAsync(d => d.Id == id);
        }

        public async Task<Domain> AddAsync(Domain entity)
        {
            return await Task.FromResult(_context.Domains.Add(entity));
        }

        public async Task<Domain> UpdateAsync(Domain entity)
        {
            var existingEntity = await _context.Domains.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            return existingEntity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var domain = await _context.Domains.FindAsync(id);
            if (domain == null)
                return false;

            _context.Domains.Remove(domain);
            return true;
        }

        public async Task<Domain> FindByNameAsync(string name)
        {
            return await _context.Domains
                .Include(d => d.ParentDomain)
                .Include(d => d.Subdomains)
                .FirstOrDefaultAsync(d => d.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Domain>> GetRootDomainsAsync()
        {
            return await _context.Domains
                .Include(d => d.Subdomains)
                .Where(d => d.ParentDomain == null)
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain>> GetSubdomainsAsync(Guid parentDomainId)
        {
            return await _context.Domains
                .Include(d => d.Subdomains)
                .Where(d => d.ParentDomain != null && d.ParentDomain.Id == parentDomainId)
                .ToListAsync();
        }

        public async Task<bool> HasBooksAsync(Guid id)
        {
            return await _context.Domains
                .Where(d => d.Id == id)
                .SelectMany(d => d.Books)
                .AnyAsync();
        }

        public async Task<bool> HasSubdomainsAsync(Guid id)
        {
            return await _context.Domains
                .AnyAsync(d => d.ParentDomain != null && d.ParentDomain.Id == id);
        }
    }
}
