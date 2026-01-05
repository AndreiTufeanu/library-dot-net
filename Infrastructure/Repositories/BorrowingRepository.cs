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
    public class BorrowingRepository : IBorrowingRepository
    {
        private readonly LibraryContext _context;

        public BorrowingRepository(LibraryContext context)
        {
            _context = context;
        }

        public async Task<Borrowing> GetByIdAsync(Guid id)
        {
            return await _context.Borrowings.FindAsync(id);
        }

        public async Task<IEnumerable<Borrowing>> GetAllAsync()
        {
            return await _context.Borrowings.ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Borrowings.AnyAsync(b => b.Id == id);
        }

        public async Task<Borrowing> AddAsync(Borrowing entity)
        {
            var addedEntity = _context.Borrowings.Add(entity);
            await _context.SaveChangesAsync();
            return addedEntity;
        }

        public async Task<Borrowing> UpdateAsync(Borrowing entity)
        {
            var existingEntity = await _context.Borrowings.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return existingEntity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var borrowing = await _context.Borrowings.FindAsync(id);
            if (borrowing == null)
                return false;

            _context.Borrowings.Remove(borrowing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Borrowing>> GetActiveByReaderAsync(Guid readerId)
        {
            return await _context.Borrowings
                .Where(b => b.Reader.Id == readerId && b.ReturnDate == null)
                .ToListAsync();
        }

        public async Task<IEnumerable<Borrowing>> GetByReaderAsync(Guid readerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Borrowings.Where(b => b.Reader.Id == readerId);

            if (startDate.HasValue)
                query = query.Where(b => b.BorrowDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(b => b.BorrowDate <= endDate.Value);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Borrowing>> GetByLibrarianAsync(Guid librarianId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Borrowings.Where(b => b.Librarian.Id == librarianId);

            if (startDate.HasValue)
                query = query.Where(b => b.BorrowDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(b => b.BorrowDate <= endDate.Value);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Borrowing>> GetOverdueAsync()
        {
            var now = DateTime.Now;
            return await _context.Borrowings
                .Where(b => b.ReturnDate == null && b.DueDate < now)
                .ToListAsync();
        }

        public async Task<IEnumerable<Borrowing>> GetByBookCopyAsync(Guid bookCopyId)
        {
            return await _context.Borrowings
                .Where(b => b.BookCopies.Any(bc => bc.Id == bookCopyId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Borrowing>> GetByBookAsync(Guid bookId)
        {
            return await _context.Borrowings
                .Where(b => b.BookCopies.Any(bc => bc.Edition.Book.Id == bookId))
                .ToListAsync();
        }

        public async Task<bool> IsBookCopyCurrentlyBorrowedAsync(Guid bookCopyId)
        {
            return await _context.Borrowings
                .AnyAsync(b => b.BookCopies.Any(bc => bc.Id == bookCopyId) && b.ReturnDate == null);
        }

        public async Task<int> GetCountByReaderInPeriodAsync(Guid readerId, DateTime startDate, DateTime endDate)
        {
            return await _context.Borrowings
                .Where(b => b.Reader.Id == readerId && b.BorrowDate >= startDate && b.BorrowDate <= endDate)
                .CountAsync();
        }

        public async Task<int> GetCountByReaderAndDomainInPeriodAsync(Guid readerId, Guid domainId, DateTime startDate)
        {
            return await _context.Borrowings
                .Where(b => b.Reader.Id == readerId && b.BorrowDate >= startDate)
                .SelectMany(b => b.BookCopies)
                .Select(bc => bc.Edition.Book)
                .Where(book => book.Domains.Any(d => d.Id == domainId))
                .CountAsync();
        }

        public async Task<int> GetTotalExtensionDaysByReaderInPeriodAsync(Guid readerId, DateTime startDate)
        {
            return await _context.Borrowings
                .Where(b => b.Reader.Id == readerId && b.BorrowDate >= startDate && b.ExtensionDays.HasValue)
                .SumAsync(b => b.ExtensionDays.Value);
        }

        public async Task<DateTime?> GetLastBorrowDateForBookByReaderAsync(Guid readerId, Guid bookId)
        {
            var lastBorrowing = await _context.Borrowings
                .Where(b => b.Reader.Id == readerId &&
                           b.BookCopies.Any(bc => bc.Edition.Book.Id == bookId))
                .OrderByDescending(b => b.BorrowDate)
                .FirstOrDefaultAsync();

            return lastBorrowing?.BorrowDate;
        }

        public async Task<int> GetCountByReaderOnDateAsync(Guid readerId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = date.Date.AddDays(1).AddTicks(-1);

            return await _context.Borrowings
                .Where(b => b.Reader.Id == readerId &&
                           b.BorrowDate >= startOfDay && b.BorrowDate <= endOfDay)
                .CountAsync();
        }

        public async Task<int> GetCountByLibrarianOnDateAsync(Guid librarianId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = date.Date.AddDays(1).AddTicks(-1);

            return await _context.Borrowings
                .Where(b => b.Librarian.Id == librarianId &&
                           b.BorrowDate >= startOfDay && b.BorrowDate <= endOfDay)
                .CountAsync();
        }
    }
}
