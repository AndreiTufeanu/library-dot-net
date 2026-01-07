using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts.HelperServiceContracts
{
    public interface IBorrowingHelperService
    {
        #region Validation Methods

        /// <summary>
        /// Validates that a borrowing does not exceed the maximum books per transaction
        /// </summary>
        /// <param name="borrowing">The borrowing to validate</param>
        /// <exception cref="Exceptions.ValidationException">Thrown when the borrowing exceeds the maximum books per transaction</exception>
        Task ValidateMaxBooksPerBorrowingAsync(Borrowing borrowing);

        /// <summary>
        /// Validates that a reader does not exceed the maximum books within the configured period
        /// </summary>
        /// <param name="borrowing">The borrowing to validate</param>
        /// <exception cref="Exceptions.ValidationException">Thrown when the reader exceeds the maximum books in period</exception>
        Task ValidateMaxBooksInPeriodAsync(Borrowing borrowing);

        /// <summary>
        /// Validates that a reader does not exceed the maximum books from the same domain
        /// </summary>
        /// <param name="borrowing">The borrowing to validate</param>
        /// <exception cref="Exceptions.ValidationException">Thrown when the reader exceeds the maximum books from same domain</exception>
        Task ValidateMaxBooksSameDomainAsync(Borrowing borrowing);

        /// <summary>
        /// Validates that a reader does not exceed the maximum sum of extension days
        /// </summary>
        /// <param name="borrowing">The borrowing to validate</param>
        /// <exception cref="Exceptions.ValidationException">Thrown when the reader exceeds the maximum overtime sum days</exception>
        Task ValidateMaxOvertimeSumDaysAsync(Borrowing borrowing);

        /// <summary>
        /// Validates that a reader respects the minimum delay before borrowing the same book again
        /// </summary>
        /// <param name="borrowing">The borrowing to validate</param>
        /// <exception cref="Exceptions.ValidationException">Thrown when the reader violates the same book delay rule</exception>
        Task ValidateSameBookDelayAsync(Borrowing borrowing);

        /// <summary>
        /// Validates that a reader does not exceed the maximum books per day
        /// </summary>
        /// <param name="borrowing">The borrowing to validate</param>
        /// <exception cref="Exceptions.ValidationException">Thrown when the reader exceeds the maximum books per day</exception>
        Task ValidateMaxBooksPerDayAsync(Borrowing borrowing);

        /// <summary>
        /// Validates that a librarian does not exceed the maximum books lent per day
        /// </summary>
        /// <param name="borrowing">The borrowing to validate</param>
        /// <exception cref="Exceptions.ValidationException">Thrown when the librarian exceeds the maximum books lent per day</exception>
        Task ValidateLibrarianLendingLimitAsync(Borrowing borrowing);

        #endregion

        #region Utility Methods

        /// <summary>
        /// Checks if a reader is also a librarian
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <returns>True if the reader is also a librarian; otherwise, false</returns>
        Task<bool> IsReaderAlsoLibrarianAsync(Guid readerId);

        /// <summary>
        /// Gets the number of books borrowed by a reader within a specific period
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <param name="startDate">The start date of the period</param>
        /// <param name="endDate">The end date of the period</param>
        /// <returns>The number of books borrowed by the reader within the specified period</returns>
        Task<int> GetBooksBorrowedInPeriodAsync(Guid readerId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets the number of books borrowed by a reader from a specific domain within a time period
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <param name="domainId">The domain identifier</param>
        /// <param name="startDate">The start date for counting</param>
        /// <returns>The number of books borrowed by the reader from the specified domain since the start date</returns>
        Task<int> GetBooksBorrowedFromDomainInPeriodAsync(Guid readerId, Guid domainId, DateTime startDate);

        /// <summary>
        /// Gets the total extension days used by a reader within a specific period
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <param name="startDate">The start date for calculating total extensions</param>
        /// <returns>The sum of all extension days used by the reader since the start date</returns>
        Task<int> GetTotalExtensionDaysInPeriodAsync(Guid readerId, DateTime startDate);

        /// <summary>
        /// Gets the last borrow date for a specific book by a reader
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <param name="bookId">The book identifier</param>
        /// <returns>The date of the last borrowing of the specified book by the reader, or null if never borrowed</returns>
        Task<DateTime?> GetLastBorrowDateForBookAsync(Guid readerId, Guid bookId);

        /// <summary>
        /// Gets the number of books borrowed by a reader on a specific date
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <param name="date">The date to check</param>
        /// <returns>The number of books borrowed by the reader on the specified date</returns>
        Task<int> GetBooksBorrowedOnDateAsync(Guid readerId, DateTime date);

        /// <summary>
        /// Gets the number of books lent by a librarian on a specific date
        /// </summary>
        /// <param name="librarianId">The librarian identifier</param>
        /// <param name="date">The date to check</param>
        /// <returns>The number of books lent by the librarian on the specified date</returns>
        Task<int> GetBooksLentByLibrarianOnDateAsync(Guid librarianId, DateTime date);

        #endregion
    }
}
