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
    public class BookCopyRepository : IBookCopyRepository
    {
        private readonly LibraryContext _context;

        public BookCopyRepository(LibraryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<BookCopy> GetByIdAsync(Guid id)
        {
            return await _context.BookCopies
                .Include(bc => bc.Edition.Book)
                .Include(bc => bc.Borrowings)
                .FirstOrDefaultAsync(bc => bc.Id == id);
        }

        public async Task<IEnumerable<BookCopy>> GetAllAsync()
        {
            return await _context.BookCopies
                .Include(bc => bc.Edition.Book)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.BookCopies.AnyAsync(bc => bc.Id == id);
        }

        public async Task<BookCopy> AddAsync(BookCopy entity)
        {
            return await Task.FromResult(_context.BookCopies.Add(entity));
        }

        public async Task<BookCopy> UpdateAsync(BookCopy entity)
        {
            var existingEntity = await _context.BookCopies.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            return existingEntity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var bookCopy = await _context.BookCopies.FindAsync(id);
            if (bookCopy == null)
                return false;

            _context.BookCopies.Remove(bookCopy);
            return true;
        }

        public async Task<IEnumerable<BookCopy>> GetBorrowableCopiesByBookAsync(Guid bookId)
        {
            return await _context.BookCopies
                .Include(bc => bc.Edition.Book)
                .Where(bc => bc.Edition.Book.Id == bookId && bc.IsBorrowable())
                .ToListAsync();
        }

        public async Task<IEnumerable<BookCopy>> GetBorrowableCopiesByEditionAsync(Guid editionId)
        {
            return await _context.BookCopies
                .Include(bc => bc.Edition)
                .Where(bc => bc.Edition.Id == editionId && bc.IsBorrowable())
                .ToListAsync();
        }

        public async Task<IEnumerable<BookCopy>> GetByBookAsync(Guid bookId)
        {
            return await _context.BookCopies
                .Include(bc => bc.Edition)
                .Where(bc => bc.Edition.Book.Id == bookId)
                .ToListAsync();
        }

        public async Task<IEnumerable<BookCopy>> GetByEditionAsync(Guid editionId)
        {
            return await _context.BookCopies
                .Include(bc => bc.Edition)
                .Where(bc => bc.Edition.Id == editionId)
                .ToListAsync();
        }

        public async Task<IEnumerable<BookCopy>> GetAvailableCopiesAsync()
        {
            return await _context.BookCopies
                .Include(bc => bc.Edition.Book)
                .Where(bc => bc.IsAvailable)
                .ToListAsync();
        }

        public async Task<IEnumerable<BookCopy>> GetLectureRoomOnlyCopiesAsync()
        {
            return await _context.BookCopies
                .Include(bc => bc.Edition.Book)
                .Where(bc => bc.IsLectureRoomOnly)
                .ToListAsync();
        }

        public async Task<bool> IsCurrentlyBorrowedAsync(Guid bookCopyId)
        {
            return await _context.Borrowings
                .AnyAsync(b => b.BookCopies.Any(bc => bc.Id == bookCopyId) && b.ReturnDate == null);
        }
    }
}
