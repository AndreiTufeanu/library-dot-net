using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.RepositoryContracts
{
    public interface IBookCopyRepository : IRepository<BookCopy>
    {
        /// <summary>
        /// Checks if a book copy is currently borrowed
        /// </summary>
        /// <param name="bookCopyId">The book copy identifier</param>
        /// <returns>True if the book copy is currently borrowed; otherwise, false</returns>
        Task<bool> IsCurrentlyBorrowedAsync(Guid bookCopyId);
    }
}
