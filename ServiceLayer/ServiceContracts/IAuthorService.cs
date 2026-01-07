using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts
{
    public interface IAuthorService : IService<Author>
    {
        /// <summary>
        /// Finds an author by first and last name
        /// </summary>
        /// <param name="firstName">The author's first name</param>
        /// <param name="lastName">The author's last name</param>
        /// <returns>A service result containing the author if found; otherwise, an error</returns>
        Task<ServiceResult<Author>> FindByNameAsync(string firstName, string lastName);

        /// <summary>
        /// Finds authors by last name
        /// </summary>
        /// <param name="lastName">The last name to search for</param>
        /// <returns>A service result containing a collection of authors with the specified last name</returns>
        Task<ServiceResult<IEnumerable<Author>>> FindByLastNameAsync(string lastName);
    }
}
