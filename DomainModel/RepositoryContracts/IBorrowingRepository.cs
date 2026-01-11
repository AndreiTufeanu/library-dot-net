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
        /// Gets the count of borrowings by a reader within a specific period
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <param name="startDate">The start date of the period</param>
        /// <param name="endDate">The end date of the period</param>
        /// <returns>The number of borrowings by the reader within the specified period</returns>
        Task<int> GetCountByReaderInPeriodAsync(Guid readerId, DateTime startDate, DateTime endDate);

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
