using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using ServiceLayer.Exceptions;
using ServiceLayer.ServiceContracts;
using ServiceLayer.ServiceContracts.HelperServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services.HelperServices
{
    public class BookHelperService : IBookHelperService
    {
        private readonly IConfigurationSettingService _configurationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDomainHelperService _domainHelperService;

        public BookHelperService(
            IConfigurationSettingService configurationService,
            IUnitOfWork unitOfWork,
            IDomainHelperService domainHelperService)
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _domainHelperService = domainHelperService ?? throw new ArgumentNullException(nameof(domainHelperService));
        }

        public async Task ValidateMaxDomainsPerBookAsync(ICollection<Domain> domains)
        {
            if (domains == null)
                throw new ArgumentNullException(nameof(domains));

            var maxDomains = await _configurationService.GetMaxDomainsPerBookAsync();

            if (domains.Count > maxDomains)
            {
                throw new AggregateValidationException(
                    $"A book cannot belong to more than {maxDomains} domains. " +
                    $"Current count: {domains.Count}");
            }
        }

        public async Task<HashSet<Guid>> GetCompleteDomainHierarchyForBookAsync(Guid bookId)
        {
            var book = await _unitOfWork.Books.GetByIdAsync(bookId);
            if (book == null)
                return new HashSet<Guid>();

            var allDomainIds = new HashSet<Guid>();

            foreach (var domain in book.Domains)
            {
                allDomainIds.Add(domain.Id);

                await _domainHelperService.PopulateAncestorDomainIdsAsync(domain.Id, allDomainIds);

                await _domainHelperService.PopulateDescendantDomainIdsAsync(domain.Id, allDomainIds);
            }

            return allDomainIds;
        }
    }
}
