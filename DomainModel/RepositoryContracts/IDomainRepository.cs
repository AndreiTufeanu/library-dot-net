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
        Task<Domain> FindByNameAsync(string name);

        /// <summary>
        /// Gets root domains (domains without parent)
        /// </summary>
        Task<IEnumerable<Domain>> GetRootDomainsAsync();

        /// <summary>
        /// Gets subdomains of a specific domain
        /// </summary>
        Task<IEnumerable<Domain>> GetSubdomainsAsync(Guid parentDomainId);

        /// <summary>
        /// Checks if a domain has associated books
        /// </summary>
        Task<bool> HasBooksAsync(Guid id);

        /// <summary>
        /// Checks if a domain has subdomains
        /// </summary>
        Task<bool> HasSubdomainsAsync(Guid id);
    }
}
