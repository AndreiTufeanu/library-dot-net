using DomainModel.RepositoryContracts;
using ServiceLayer.ServiceContracts.HelperServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services.HelperServices
{
    /// <summary>Provides helper methods for recursive domain hierarchy traversal and relationship analysis.</summary>
    /// <remarks>
    /// Implements <see cref="IDomainHelperService"/> to support domain-related business rules.
    /// Provides recursive logic for traversing domain hierarchies both upwards (ancestors) and downwards (descendants).
    /// </remarks>
    public class DomainHelperService : IDomainHelperService
    {
        /// <summary>The unit of work instance for accessing domain repository operations.</summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>Initializes a new instance of the <see cref="DomainHelperService"/> class.</summary>
        /// <param name="unitOfWork">The unit of work instance for accessing domain repository operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="unitOfWork"/> is null.</exception>
        public DomainHelperService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <inheritdoc/>
        public async Task PopulateAncestorDomainIdsAsync(Guid domainId, HashSet<Guid> domainIds)
        {
            var domain = await _unitOfWork.Domains.GetByIdAsync(domainId);
            if (domain?.ParentDomain != null)
            {
                domainIds.Add(domain.ParentDomain.Id);
                await PopulateAncestorDomainIdsAsync(domain.ParentDomain.Id, domainIds);
            }
        }

        /// <inheritdoc/>
        public async Task PopulateDescendantDomainIdsAsync(Guid domainId, HashSet<Guid> domainIds)
        {
            var subdomains = await _unitOfWork.Domains.GetSubdomainsAsync(domainId);
            foreach (var subdomain in subdomains)
            {
                if (!domainIds.Contains(subdomain.Id))
                {
                    domainIds.Add(subdomain.Id);
                    await PopulateDescendantDomainIdsAsync(subdomain.Id, domainIds);
                }
            }
        }
    }
}
