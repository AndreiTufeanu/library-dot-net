using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.RepositoryContracts
{
    public interface ILibrarianRepository : IRepository<Librarian>
    {
        /// <summary>
        /// Gets librarian by reader ID (1:1 relationship)
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <returns>The librarian associated with the reader, or null if not found</returns>
        Task<Librarian> GetByReaderIdAsync(Guid readerId);

        /// <summary>
        /// Checks if a reader is also a librarian
        /// </summary>
        /// <param name="readerId">The reader identifier</param>
        /// <returns>True if the reader is also a librarian; otherwise, false</returns>
        Task<bool> IsReaderAlsoLibrarianAsync(Guid readerId);
    }
}
