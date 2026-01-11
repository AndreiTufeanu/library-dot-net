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
        /// Checks if a book has physical copies
        /// </summary>
        /// <param name="id">The book identifier</param>
        /// <returns>True if the book has at least one physical copy; otherwise, false</returns>
        Task<bool> HasPhysicalCopiesAsync(Guid id);
    }
}
