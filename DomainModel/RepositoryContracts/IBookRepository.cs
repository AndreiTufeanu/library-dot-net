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
        /// <param name="title">The title or partial title to search for</param>
        /// <returns>A collection of books matching the title criteria</returns>
        Task<IEnumerable<Book>> FindByTitleAsync(string title);

        /// <summary>
        /// Finds books by domain (including subdomains)
        /// </summary>
        /// <param name="domainId">The domain identifier</param>
        /// <returns>A collection of books belonging to the specified domain or its subdomains</returns>
        Task<IEnumerable<Book>> FindByDomainAsync(Guid domainId);

        /// <summary>
        /// Finds books by author
        /// </summary>
        /// <param name="authorId">The author identifier</param>
        /// <returns>A collection of books written by the specified author</returns>
        Task<IEnumerable<Book>> FindByAuthorAsync(Guid authorId);

        /// <summary>
        /// Gets books currently available for borrowing according to business rules
        /// </summary>
        /// <returns>A collection of books that satisfy the borrowing availability criteria</returns>
        Task<IEnumerable<Book>> GetAvailableForBorrowingAsync();

        /// <summary>
        /// Checks if a book has physical copies
        /// </summary>
        /// <param name="id">The book identifier</param>
        /// <returns>True if the book has at least one physical copy; otherwise, false</returns>
        Task<bool> HasPhysicalCopiesAsync(Guid id);
    }
}
