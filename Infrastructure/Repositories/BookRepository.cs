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
    public class BookRepository : IBookRepository
    {
        private readonly LibraryContext _context;

        public BookRepository(LibraryContext context)
        {
            _context = context;
        }

        public async Task<Book> GetByIdAsync(Guid id)
        {
            return await _context.Books.FindAsync(id);
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _context.Books.ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Books.AnyAsync(b => b.Id == id);
        }

        public async Task<Book> AddAsync(Book entity)
        {
            var addedEntity = _context.Books.Add(entity);
            await _context.SaveChangesAsync();
            return addedEntity;
        }

        public async Task<Book> UpdateAsync(Book entity)
        {
            var existingEntity = await _context.Books.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return existingEntity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return false;

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Book>> FindByTitleAsync(string title)
        {
            return await _context.Books
                .Where(b => b.Title.ToLower().Contains(title.ToLower()))
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> FindByDomainAsync(Guid domainId)
        {
            return await _context.Books
                .Where(b => b.Domains.Any(d => d.Id == domainId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> FindByAuthorAsync(Guid authorId)
        {
            return await _context.Books
                .Where(b => b.Authors.Any(a => a.Id == authorId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetAvailableForBorrowingAsync()
        {
            return await _context.Books
                .Where(b => b.IsAvailableForBorrowing())
                .ToListAsync();
        }

        public async Task<bool> HasPhysicalCopiesAsync(Guid id)
        {
            return await _context.Books
                .Where(b => b.Id == id)
                .SelectMany(b => b.Editions)
                .SelectMany(e => e.BookCopies)
                .AnyAsync();
        }
    }
}
