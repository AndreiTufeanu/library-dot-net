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
