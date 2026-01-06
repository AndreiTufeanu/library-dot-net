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
    public class AuthorRepository : IAuthorRepository
    {
        private readonly LibraryContext _context;

        public AuthorRepository(LibraryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Author> GetByIdAsync(Guid id)
        {
            return await _context.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Author>> GetAllAsync()
        {
            return await _context.Authors
                .Include(a => a.Books)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Authors.AnyAsync(a => a.Id == id);
        }

        public async Task<Author> AddAsync(Author entity)
        {
            return await Task.FromResult(_context.Authors.Add(entity));
        }

        public async Task<Author> UpdateAsync(Author entity)
        {
            var existingEntity = await _context.Authors.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            return existingEntity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null)
                return false;

            _context.Authors.Remove(author);
            return true;
        }

        public async Task<Author> FindByNameAsync(string firstName, string lastName)
        {
            return await _context.Authors
                .FirstOrDefaultAsync(a =>
                    a.FirstName.ToLower() == firstName.ToLower() &&
                    a.LastName.ToLower() == lastName.ToLower());
        }

        public async Task<IEnumerable<Author>> FindByLastNameAsync(string lastName)
        {
            return await _context.Authors
                .Where(a => a.LastName.ToLower() == lastName.ToLower())
                .ToListAsync();
        }

        public async Task<bool> HasBooksAsync(Guid id)
        {
            return await _context.Authors
                .Where(a => a.Id == id)
                .SelectMany(a => a.Books)
                .AnyAsync();
        }
    }
}
