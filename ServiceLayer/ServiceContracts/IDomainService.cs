using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts
{
    public interface IDomainService : IService<Domain>
    {
        /// <summary>
        /// Finds a domain by name
        /// </summary>
        /// <param name="name">The domain name to search for</param>
        /// <returns>A service result containing the domain if found; otherwise, an error</returns>
        Task<ServiceResult<Domain>> FindByNameAsync(string name);

        /// <summary>
        /// Gets root domains (domains without parent)
        /// </summary>
        /// <returns>A service result containing a collection of root domains</returns>
        Task<ServiceResult<IEnumerable<Domain>>> GetRootDomainsAsync();

        /// <summary>
        /// Gets subdomains of a specific domain
        /// </summary>
        /// <param name="parentDomainId">The parent domain identifier</param>
        /// <returns>A service result containing a collection of subdomains for the specified parent domain</returns>
        Task<ServiceResult<IEnumerable<Domain>>> GetSubdomainsAsync(Guid parentDomainId);
    }
}
