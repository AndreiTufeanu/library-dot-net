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
        /// Finds readers by last name (case-insensitive)
        /// </summary>
        Task<IEnumerable<Reader>> FindByLastNameAsync(string lastName);

        /// <summary>
        /// Finds reader by email (case-insensitive)
        /// </summary>
        Task<Reader> FindByEmailAsync(string email);

        /// <summary>
        /// Finds reader by phone number
        /// </summary>
        Task<Reader> FindByPhoneAsync(string phoneNumber);

        /// <summary>
        /// Checks if a reader has active borrowings
        /// </summary>
        Task<bool> HasActiveBorrowingsAsync(Guid id);
    }
}
