using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.RepositoryContracts
{
    public interface IBookRepository : IRepository<Book>
    {
        /// <summary>
        /// Finds books by title (case-insensitive, partial match)
        /// </summary>
        Task<IEnumerable<Book>> FindByTitleAsync(string title);

        /// <summary>
        /// Finds books by domain (including subdomains)
        /// </summary>
        Task<IEnumerable<Book>> FindByDomainAsync(Guid domainId);

        /// <summary>
        /// Finds books by author
        /// </summary>
        Task<IEnumerable<Book>> FindByAuthorAsync(Guid authorId);

        /// <summary>
        /// Gets books available for borrowing
        /// </summary>
        Task<IEnumerable<Book>> GetAvailableForBorrowingAsync();

        /// <summary>
        /// Checks if a book has physical copies
        /// </summary>
        Task<bool> HasPhysicalCopiesAsync(Guid id);
    }
}
