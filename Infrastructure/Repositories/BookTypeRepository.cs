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
    public class BookTypeRepository : IBookTypeRepository
    {
        private readonly LibraryContext _context;

        public BookTypeRepository(LibraryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<BookType> GetByIdAsync(Guid id)
        {
            return await _context.BookTypes
                .Include(bt => bt.Editions)
                .FirstOrDefaultAsync(bt => bt.Id == id);
        }

        public async Task<IEnumerable<BookType>> GetAllAsync()
        {
            return await _context.BookTypes
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.BookTypes.AnyAsync(bt => bt.Id == id);
        }

        public async Task<BookType> FindByNameAsync(string name)
        {
            return await _context.BookTypes
                .FirstOrDefaultAsync(bt => bt.Name.ToLower() == name.ToLower());
        }

        public async Task<BookType> AddAsync(BookType entity)
        {
            var addedEntity = _context.BookTypes.Add(entity);
            return await Task.FromResult(addedEntity);
        }

        public async Task<BookType> UpdateAsync(BookType entity)
        {
            var existingEntity = await _context.BookTypes.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            return existingEntity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var bookType = await _context.BookTypes.FindAsync(id);
            if (bookType == null)
                return false;

            _context.BookTypes.Remove(bookType);
            return true;
        }

        public async Task<bool> HasEditionsAsync(Guid id)
        {
            return await _context.BookTypes
                .Where(bt => bt.Id == id)
                .SelectMany(bt => bt.Editions)
                .AnyAsync();
        }
    }
}
