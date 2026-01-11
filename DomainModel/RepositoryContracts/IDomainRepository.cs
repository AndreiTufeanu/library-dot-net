using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.RepositoryContracts
{
    public interface IDomainRepository : IRepository<Domain>
    {
        /// <summary>
        /// Finds a domain by name (case-insensitive)
        /// </summary>
        /// <param name="name">The domain name to search for</param>
        /// <returns>The domain if found; otherwise, null</returns>
        Task<Domain> FindByNameAsync(string name);

        /// <summary>
        /// Gets subdomains of a specific domain
        /// </summary>
        /// <param name="parentDomainId">The parent domain identifier</param>
        /// <returns>A collection of subdomains for the specified parent domain</returns>
        Task<IEnumerable<Domain>> GetSubdomainsAsync(Guid parentDomainId);

        /// <summary>
        /// Checks if a domain has associated books
        /// </summary>
        /// <param name="id">The domain identifier</param>
        /// <returns>True if the domain has associated books; otherwise, false</returns>
        Task<bool> HasBooksAsync(Guid id);

        /// <summary>
        /// Checks if a domain has subdomains
        /// </summary>
        /// <param name="id">The domain identifier</param>
        /// <returns>True if the domain has subdomains; otherwise, false</returns>
        Task<bool> HasSubdomainsAsync(Guid id);
    }
}
