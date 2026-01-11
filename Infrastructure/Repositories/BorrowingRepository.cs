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
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Borrowing> GetByIdAsync(Guid id)
        {
            return await _context.Borrowings
                .Include(b => b.Reader)
                .Include(b => b.Librarian)
                .Include(b => b.BookCopies.Select(bc => bc.Edition.Book.Domains))
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Borrowing>> GetAllAsync()
        {
            return await _context.Borrowings
                .Include(b => b.Reader)
                .Include(b => b.Librarian)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Borrowings.AnyAsync(b => b.Id == id);
        }

        public async Task<Borrowing> AddAsync(Borrowing entity)
        {
            return await Task.FromResult(_context.Borrowings.Add(entity));
        }

        public async Task<Borrowing> UpdateAsync(Borrowing entity)
        {
            var existingEntity = await _context.Borrowings.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            return existingEntity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var borrowing = await _context.Borrowings.FindAsync(id);
            if (borrowing == null)
                return false;

            _context.Borrowings.Remove(borrowing);
            return true;
        }

        public async Task<int> GetCountByReaderInPeriodAsync(Guid readerId, DateTime startDate, DateTime endDate)
        {
            return await _context.Borrowings
                .Where(b => b.Reader.Id == readerId && b.BorrowDate >= startDate && b.BorrowDate <= endDate)
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

        public async Task<int> GetCountByReaderAndDomainsInPeriodAsync(
            Guid readerId,
            List<Guid> domainIds,
            DateTime startDate)
        {
            return await _context.Borrowings
                .Where(b => b.Reader.Id == readerId && b.BorrowDate >= startDate)
                .Where(b => b.BookCopies.Any(bc =>
                    bc.Edition.Book.Domains.Any(d => domainIds.Contains(d.Id))))
                .CountAsync();
        }
    }
}
