using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.RepositoryContracts
{
    public interface IBorrowingRepository : IRepository<Borrowing>
    {
        /// <summary>
        /// Gets active (not returned) borrowings for a reader
        /// </summary>
        Task<IEnumerable<Borrowing>> GetActiveByReaderAsync(Guid readerId);

        /// <summary>
        /// Gets all borrowings for a reader (with optional date range)
        /// </summary>
        Task<IEnumerable<Borrowing>> GetByReaderAsync(Guid readerId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets borrowings by librarian
        /// </summary>
        Task<IEnumerable<Borrowing>> GetByLibrarianAsync(Guid librarianId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets overdue borrowings (active and past due date)
        /// </summary>
        Task<IEnumerable<Borrowing>> GetOverdueAsync();

        /// <summary>
        /// Gets borrowings for a specific book copy
        /// </summary>
        Task<IEnumerable<Borrowing>> GetByBookCopyAsync(Guid bookCopyId);

        /// <summary>
        /// Gets borrowings for a specific book (across all copies)
        /// </summary>
        Task<IEnumerable<Borrowing>> GetByBookAsync(Guid bookId);

        /// <summary>
        /// Checks if a book copy is currently borrowed
        /// </summary>
        Task<bool> IsBookCopyCurrentlyBorrowedAsync(Guid bookCopyId);

        /// <summary>
        /// Gets count of borrowings for a reader in a date range
        /// </summary>
        Task<int> GetCountByReaderInPeriodAsync(Guid readerId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets count of borrowings by domain for a reader in a date range
        /// </summary>
        Task<int> GetCountByReaderAndDomainInPeriodAsync(Guid readerId, Guid domainId, DateTime startDate);

        /// <summary>
        /// Gets total extension days for a reader in a date range
        /// </summary>
        Task<int> GetTotalExtensionDaysByReaderInPeriodAsync(Guid readerId, DateTime startDate);

        /// <summary>
        /// Gets last borrowing date for a specific book by a reader
        /// </summary>
        Task<DateTime?> GetLastBorrowDateForBookByReaderAsync(Guid readerId, Guid bookId);

        /// <summary>
        /// Gets count of borrowings by a reader on a specific date
        /// </summary>
        Task<int> GetCountByReaderOnDateAsync(Guid readerId, DateTime date);

        /// <summary>
        /// Gets count of borrowings processed by a librarian on a specific date
        /// </summary>
        Task<int> GetCountByLibrarianOnDateAsync(Guid librarianId, DateTime date);
    }
}
