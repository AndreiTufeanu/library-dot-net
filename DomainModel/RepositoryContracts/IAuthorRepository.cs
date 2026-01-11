using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.RepositoryContracts
{
    public interface IAuthorRepository : IRepository<Author>
    {
        /// <summary>
        /// Finds an author by first and last name (case-insensitive)
        /// </summary>
        /// <param name="firstName">The author's first name</param>
        /// <param name="lastName">The author's last name</param>
        /// <returns>The author if found; otherwise, null</returns>
        Task<Author> FindByNameAsync(string firstName, string lastName);

        /// <summary>
        /// Checks if an author has associated books
        /// </summary>
        /// <param name="id">The author identifier</param>
        /// <returns>True if the author has associated books; otherwise, false</returns>
        Task<bool> HasBooksAsync(Guid id);
    }
}
