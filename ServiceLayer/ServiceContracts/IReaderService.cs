using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts
{
    public interface IReaderService : IService<Reader>
    {
        /// <summary>
        /// Finds reader by email
        /// </summary>
        /// <param name="email">The email address to search for</param>
        /// <returns>A service result containing the reader with the specified email if found; otherwise, an error</returns>
        Task<ServiceResult<Reader>> FindByEmailAsync(string email);

        /// <summary>
        /// Finds reader by phone number
        /// </summary>
        /// <param name="phoneNumber">The phone number to search for</param>
        /// <returns>A service result containing the reader with the specified phone number if found; otherwise, an error</returns>
        Task<ServiceResult<Reader>> FindByPhoneAsync(string phoneNumber);

        /// <summary>
        /// Finds readers by last name
        /// </summary>
        /// <param name="lastName">The last name to search for</param>
        /// <returns>A service result containing a collection of readers with the specified last name</returns>
        Task<ServiceResult<IEnumerable<Reader>>> FindByLastNameAsync(string lastName);

        /// <summary>
        /// Checks if a reader has active borrowings
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <returns>A service result containing true if the reader has active borrowings; otherwise, false</returns>
        Task<ServiceResult<bool>> HasActiveBorrowingsAsync(Guid readerId);
    }
}
