using DomainModel.Entities;
using DomainModel.RepositoryContracts.DomainModel.RepositoryContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.RepositoryContracts
{
    public interface IBookTypeRepository : IRepository<BookType>
    {
        /// <summary>
        /// Finds a book type by its name (case-insensitive)
        /// </summary>
        /// <param name="name">The book type name to search for</param>
        /// <returns>The book type if found, otherwise null</returns>
        Task<BookType> FindByNameAsync(string name);

        /// <summary>
        /// Checks if a book type has associated editions
        /// </summary>
        /// <param name="id">The book type identifier</param>
        /// <returns>True if the book type has editions, otherwise false</returns>
        Task<bool> HasEditionsAsync(Guid id);
    }
}
