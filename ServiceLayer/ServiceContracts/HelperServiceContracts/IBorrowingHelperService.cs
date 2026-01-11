using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLayer.Exceptions;

namespace ServiceLayer.ServiceContracts.HelperServiceContracts
{
    public interface IBorrowingHelperService
    {
        #region Validation Methods

        /// <summary>
        /// Validates that a borrowing does not exceed the maximum books per transaction
        /// </summary>
        /// <param name="borrowing">The borrowing to validate</param>
        /// <exception cref="AggregateValidationException">Thrown when the borrowing exceeds the maximum books per transaction</exception>
        Task ValidateMaxBooksPerBorrowingAsync(Borrowing borrowing);

        /// <summary>
        /// Validates that a reader does not exceed the maximum books within the configured period
        /// </summary>
        /// <param name="borrowing">The borrowing to validate</param>
        /// <exception cref="AggregateValidationException">Thrown when the reader exceeds the maximum books in period</exception>
        Task ValidateMaxBooksInPeriodAsync(Borrowing borrowing);

        /// <summary>
        /// Validates that a reader does not exceed the maximum books from the same domain
        /// </summary>
        /// <param name="borrowing">The borrowing to validate</param>
        /// <exception cref="AggregateValidationException">Thrown when the reader exceeds the maximum books from same domain</exception>
        Task ValidateMaxBooksSameDomainAsync(Borrowing borrowing);

        /// <summary>
        /// Validates that a reader does not exceed the maximum sum of extension days
        /// </summary>
        /// <param name="borrowing">The borrowing to validate</param>
        /// <exception cref="AggregateValidationException">Thrown when the reader exceeds the maximum overtime sum days</exception>
        Task ValidateMaxOvertimeSumDaysAsync(Borrowing borrowing);

        /// <summary>
        /// Validates that a reader respects the minimum delay before borrowing the same book again
        /// </summary>
        /// <param name="borrowing">The borrowing to validate</param>
        /// <exception cref="AggregateValidationException">Thrown when the reader violates the same book delay rule</exception>
        Task ValidateSameBookDelayAsync(Borrowing borrowing);

        /// <summary>
        /// Validates that a reader does not exceed the maximum books per day
        /// </summary>
        /// <param name="borrowing">The borrowing to validate</param>
        /// <exception cref="AggregateValidationException">Thrown when the reader exceeds the maximum books per day</exception>
        Task ValidateMaxBooksPerDayAsync(Borrowing borrowing);

        /// <summary>
        /// Validates that a librarian does not exceed the maximum books lent per day
        /// </summary>
        /// <param name="borrowing">The borrowing to validate</param>
        /// <exception cref="AggregateValidationException">Thrown when the librarian exceeds the maximum books lent per day</exception>
        Task ValidateLibrarianLendingLimitAsync(Borrowing borrowing);

        #endregion

        #region Utility Methods

        /// <summary>
        /// Checks if a reader is also a librarian
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <returns>True if the reader is also a librarian; otherwise, false</returns>
        Task<bool> IsReaderAlsoLibrarianAsync(Guid readerId);

        #endregion
    }
}
