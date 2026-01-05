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
        Task<Librarian> GetByReaderIdAsync(Guid readerId);

        /// <summary>
        /// Checks if a reader is also a librarian
        /// </summary>
        Task<bool> IsReaderAlsoLibrarianAsync(Guid readerId);
    }
}
