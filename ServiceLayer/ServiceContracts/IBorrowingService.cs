using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts
{
    public interface IBorrowingService : IService<Borrowing>
    {
        /// <summary>
        /// Gets active borrowings for a specific reader
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <returns>A service result containing a collection of active (not returned) borrowings for the specified reader</returns>
        Task<ServiceResult<IEnumerable<Borrowing>>> GetActiveByReaderAsync(Guid readerId);

        /// <summary>
        /// Gets borrowings for a specific reader within an optional date range
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <param name="startDate">Optional start date for filtering</param>
        /// <param name="endDate">Optional end date for filtering</param>
        /// <returns>A service result containing a collection of borrowings for the specified reader within the date range</returns>
        Task<ServiceResult<IEnumerable<Borrowing>>> GetByReaderAsync(Guid readerId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets borrowings processed by a specific librarian within an optional date range
        /// </summary>
        /// <param name="librarianId">The librarian identifier</param>
        /// <param name="startDate">Optional start date for filtering</param>
        /// <param name="endDate">Optional end date for filtering</param>
        /// <returns>A service result containing a collection of borrowings processed by the specified librarian within the date range</returns>
        Task<ServiceResult<IEnumerable<Borrowing>>> GetByLibrarianAsync(Guid librarianId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets overdue borrowings (active borrowings with due date passed)
        /// </summary>
        /// <returns>A service result containing a collection of overdue borrowings</returns>
        Task<ServiceResult<IEnumerable<Borrowing>>> GetOverdueAsync();

        /// <summary>
        /// Gets borrowings for a specific book copy
        /// </summary>
        /// <param name="bookCopyId">The book copy identifier</param>
        /// <returns>A service result containing a collection of borrowings that include the specified book copy</returns>
        Task<ServiceResult<IEnumerable<Borrowing>>> GetByBookCopyAsync(Guid bookCopyId);

        /// <summary>
        /// Gets borrowings for a specific book (any copy)
        /// </summary>
        /// <param name="bookId">The book identifier</param>
        /// <returns>A service result containing a collection of borrowings that include any copy of the specified book</returns>
        Task<ServiceResult<IEnumerable<Borrowing>>> GetByBookAsync(Guid bookId);

        /// <summary>
        /// Finishes a borrowing by marking it as returned
        /// </summary>
        /// <param name="borrowingId">The borrowing identifier</param>
        /// <param name="returnDate">Optional return date (uses current date if not specified)</param>
        /// <returns>A service result indicating success or failure of the operation</returns>
        Task<ServiceResult<bool>> FinishBorrowingAsync(Guid borrowingId, DateTime? returnDate = null);

        /// <summary>
        /// Extends a borrowing by adding extension days
        /// </summary>
        /// <param name="borrowingId">The borrowing identifier</param>
        /// <param name="extensionDays">The number of days to extend the borrowing</param>
        /// <returns>A service result indicating success or failure of the operation</returns>
        Task<ServiceResult<bool>> ExtendBorrowingAsync(Guid borrowingId, int extensionDays);
    }
}
