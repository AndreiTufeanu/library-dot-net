using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts
{
    public interface ILibrarianService : IService<Librarian>
    {
        /// <summary>
        /// Gets librarian by reader ID (1:1 relationship)
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <returns>A service result containing the librarian associated with the reader if found; otherwise, an error</returns>
        Task<ServiceResult<Librarian>> GetByReaderIdAsync(Guid readerId);

        /// <summary>
        /// Checks if a reader is also a librarian
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <returns>A service result containing true if the reader is also a librarian; otherwise, false</returns>
        Task<ServiceResult<bool>> IsReaderAlsoLibrarianAsync(Guid readerId);

        /// <summary>
        /// Creates a librarian from an existing reader
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <returns>A service result containing the newly created librarian</returns>
        Task<ServiceResult<Librarian>> CreateFromReaderAsync(Guid readerId);

        /// <summary>
        /// Removes librarian status from a user
        /// </summary>
        /// <param name="librarianId">The librarian identifier</param>
        /// <returns>A service result indicating success or failure of the operation</returns>
        Task<ServiceResult<bool>> RemoveLibrarianStatusAsync(Guid librarianId);
    }
}
