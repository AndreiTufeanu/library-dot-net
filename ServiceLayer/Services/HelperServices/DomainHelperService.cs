using DomainModel.RepositoryContracts;
using ServiceLayer.ServiceContracts.HelperServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services.HelperServices
{
    public class DomainHelperService : IDomainHelperService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DomainHelperService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task PopulateAncestorDomainIdsAsync(Guid domainId, HashSet<Guid> domainIds)
        {
            var domain = await _unitOfWork.Domains.GetByIdAsync(domainId);
            if (domain?.ParentDomain != null)
            {
                domainIds.Add(domain.ParentDomain.Id);
                await PopulateAncestorDomainIdsAsync(domain.ParentDomain.Id, domainIds);
            }
        }

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
