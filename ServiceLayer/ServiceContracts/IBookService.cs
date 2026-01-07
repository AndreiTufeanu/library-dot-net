using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts
{
    public interface IBookService : IService<Book>
    {
        /// <summary>
        /// Finds books by title (case-insensitive, partial match)
        /// </summary>
        /// <param name="title">The title or partial title to search for</param>
        /// <returns>A service result containing a collection of books matching the title criteria</returns>
        Task<ServiceResult<IEnumerable<Book>>> FindByTitleAsync(string title);

        /// <summary>
        /// Finds books by domain (including subdomains)
        /// </summary>
        /// <param name="domainId">The domain identifier</param>
        /// <returns>A service result containing a collection of books belonging to the specified domain or its subdomains</returns>
        Task<ServiceResult<IEnumerable<Book>>> FindByDomainAsync(Guid domainId);

        /// <summary>
        /// Finds books by author
        /// </summary>
        /// <param name="authorId">The author identifier</param>
        /// <returns>A service result containing a collection of books written by the specified author</returns>
        Task<ServiceResult<IEnumerable<Book>>> FindByAuthorAsync(Guid authorId);

        /// <summary>
        /// Gets books currently available for borrowing according to business rules
        /// </summary>
        /// <returns>A service result containing a collection of books that satisfy the borrowing availability criteria</returns>
        Task<ServiceResult<IEnumerable<Book>>> GetAvailableForBorrowingAsync();
    }
}
