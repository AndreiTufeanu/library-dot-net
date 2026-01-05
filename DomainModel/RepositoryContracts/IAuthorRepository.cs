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
        Task<Author> FindByNameAsync(string firstName, string lastName);

        /// <summary>
        /// Finds authors by last name (case-insensitive)
        /// </summary>
        Task<IEnumerable<Author>> FindByLastNameAsync(string lastName);

        /// <summary>
        /// Checks if an author has associated books
        /// </summary>
        Task<bool> HasBooksAsync(Guid id);
    }
}
