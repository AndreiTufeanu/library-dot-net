using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.RepositoryContracts
{
    public interface IReaderRepository : IRepository<Reader>
    {
        /// <summary>
        /// Finds reader by email (case-insensitive)
        /// </summary>
        /// <param name="email">The email address to search for</param>
        /// <returns>The reader with the specified email, or null if not found</returns>
        Task<Reader> FindByEmailAsync(string email);

        /// <summary>
        /// Finds reader by phone number
        /// </summary>
        /// <param name="phoneNumber">The phone number to search for</param>
        /// <returns>The reader with the specified phone number, or null if not found</returns>
        Task<Reader> FindByPhoneAsync(string phoneNumber);

        /// <summary>
        /// Checks if a reader has active borrowings
        /// </summary>
        /// <param name="id">The reader identifier</param>
        /// <returns>True if the reader has active (not returned) borrowings; otherwise, false</returns>
        Task<bool> HasActiveBorrowingsAsync(Guid id);
    }
}
