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
        /// Gets active borrowings for a specific reader
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <returns>A collection of active (not returned) borrowings for the specified reader</returns>
        Task<IEnumerable<Borrowing>> GetActiveByReaderAsync(Guid readerId);

        /// <summary>
        /// Gets borrowings for a specific reader within an optional date range
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <param name="startDate">Optional start date for filtering</param>
        /// <param name="endDate">Optional end date for filtering</param>
        /// <returns>A collection of borrowings for the specified reader within the date range</returns>
        Task<IEnumerable<Borrowing>> GetByReaderAsync(Guid readerId, DateTime? startDate = null, DateTime? endDate = null);
        
        /// <summary>
        /// Gets borrowings processed by a specific librarian within an optional date range
        /// </summary>
        /// <param name="librarianId">The librarian identifier</param>
        /// <param name="startDate">Optional start date for filtering</param>
        /// <param name="endDate">Optional end date for filtering</param>
        /// <returns>A collection of borrowings processed by the specified librarian within the date range</returns>
        Task<IEnumerable<Borrowing>> GetByLibrarianAsync(Guid librarianId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets overdue borrowings (active borrowings with due date passed)
        /// </summary>
        /// <returns>A collection of overdue borrowings</returns>
        Task<IEnumerable<Borrowing>> GetOverdueAsync();

        /// <summary>
        /// Gets borrowings for a specific book copy
        /// </summary>
        /// <param name="bookCopyId">The book copy identifier</param>
        /// <returns>A collection of borrowings that include the specified book copy</returns>
        Task<IEnumerable<Borrowing>> GetByBookCopyAsync(Guid bookCopyId);

        /// <summary>
        /// Gets borrowings for a specific book (any copy)
        /// </summary>
        /// <param name="bookId">The book identifier</param>
        /// <returns>A collection of borrowings that include any copy of the specified book</returns>
        Task<IEnumerable<Borrowing>> GetByBookAsync(Guid bookId);

        /// <summary>
        /// Checks if a book copy is currently borrowed
        /// </summary>
        /// <param name="bookCopyId">The book copy identifier</param>
        /// <returns>True if the book copy is currently borrowed; otherwise, false</returns>
        Task<bool> IsBookCopyCurrentlyBorrowedAsync(Guid bookCopyId);

        /// <summary>
        /// Gets the count of borrowings by a reader within a specific period
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <param name="startDate">The start date of the period</param>
        /// <param name="endDate">The end date of the period</param>
        /// <returns>The number of borrowings by the reader within the specified period</returns>
        Task<int> GetCountByReaderInPeriodAsync(Guid readerId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets the count of borrowings by a reader from a specific domain within a time period
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <param name="domainId">The domain identifier</param>
        /// <param name="startDate">The start date for counting (typically current date minus time window)</param>
        /// <returns>The number of borrowings by the reader from the specified domain since the start date</returns>
        Task<int> GetCountByReaderAndDomainInPeriodAsync(Guid readerId, Guid domainId, DateTime startDate);

        /// <summary>
        /// Gets the total extension days used by a reader within a specific period
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <param name="startDate">The start date for calculating total extensions</param>
        /// <returns>The sum of all extension days used by the reader since the start date</returns>
        Task<int> GetTotalExtensionDaysByReaderInPeriodAsync(Guid readerId, DateTime startDate);

        /// <summary>
        /// Gets the last borrow date for a specific book by a reader
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <param name="bookId">The book identifier</param>
        /// <returns>The date of the last borrowing of the specified book by the reader, or null if never borrowed</returns>
        Task<DateTime?> GetLastBorrowDateForBookByReaderAsync(Guid readerId, Guid bookId);

        /// <summary>
        /// Gets the count of borrowings by a reader on a specific date
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <param name="date">The date to check</param>
        /// <returns>The number of borrowings by the reader on the specified date</returns>
        Task<int> GetCountByReaderOnDateAsync(Guid readerId, DateTime date);

        /// <summary>
        /// Gets the count of borrowings processed by a librarian on a specific date
        /// </summary>
        /// <param name="librarianId">The librarian identifier</param>
        /// <param name="date">The date to check</param>
        /// <returns>The number of borrowings processed by the librarian on the specified date</returns>
        Task<int> GetCountByLibrarianOnDateAsync(Guid librarianId, DateTime date);

        /// <summary>
        /// Gets the count of borrowings by a reader from specified domains within a time period
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <param name="domainIds">The list of domain identifiers to check</param>
        /// <param name="startDate">The start date for counting</param>
        /// <returns>The number of borrowings by the reader from any of the specified domains since the start date</returns>
        Task<int> GetCountByReaderAndDomainsInPeriodAsync(Guid readerId, List<Guid> domainIds, DateTime startDate);
    }
}
