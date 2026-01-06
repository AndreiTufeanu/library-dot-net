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
    public class EditionRepository : IEditionRepository
    {
        private readonly LibraryContext _context;

        public EditionRepository(LibraryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Edition> GetByIdAsync(Guid id)
        {
            return await _context.Editions
                .Include(e => e.Book)
                .Include(e => e.BookType)
                .Include(e => e.BookCopies)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Edition>> GetAllAsync()
        {
            return await _context.Editions
                .Include(e => e.Book)
                .Include(e => e.BookType)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Editions.AnyAsync(e => e.Id == id);
        }

        public async Task<Edition> AddAsync(Edition entity)
        {
            return await Task.FromResult(_context.Editions.Add(entity));
        }

        public async Task<Edition> UpdateAsync(Edition entity)
        {
            var existingEntity = await _context.Editions.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            return existingEntity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var edition = await _context.Editions.FindAsync(id);
            if (edition == null)
                return false;

            _context.Editions.Remove(edition);
            return true;
        }

        public async Task<IEnumerable<Edition>> GetByBookAsync(Guid bookId)
        {
            return await _context.Editions
                .Include(e => e.Book)
                .Include(e => e.BookType)
                .Where(e => e.Book.Id == bookId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Edition>> GetByBookTypeAsync(Guid bookTypeId)
        {
            return await _context.Editions
                .Include(e => e.Book)
                .Include(e => e.BookType)
                .Where(e => e.BookType.Id == bookTypeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Edition>> GetPublishedBetweenAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Editions
                .Include(e => e.Book)
                .Include(e => e.BookType)
                .Where(e => e.PublicationDate >= startDate && e.PublicationDate <= endDate)
                .ToListAsync();
        }

        public async Task<bool> HasCopiesAsync(Guid id)
        {
            return await _context.Editions
                .Where(e => e.Id == id)
                .SelectMany(e => e.BookCopies)
                .AnyAsync();
        }
    }
}
