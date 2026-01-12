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
    /// <summary>Provides helper methods for book-related business rule validation and domain hierarchy operations.</summary>
    /// <remarks>
    /// Implements <see cref="IBookHelperService"/> to support book service operations.
    /// Focuses on domain-related rules including maximum domains per book and complete domain hierarchy retrieval.
    /// </remarks>
    public class BookHelperService : IBookHelperService
    {
        /// <summary>The configuration service for retrieving business rule parameters.</summary>
        private readonly IConfigurationSettingService _configurationService;

        /// <summary>The unit of work instance for accessing repository operations.</summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>The helper service for domain hierarchy operations.</summary>
        private readonly IDomainHelperService _domainHelperService;

        /// <summary>Initializes a new instance of the <see cref="BookHelperService"/> class.</summary>
        /// <param name="configurationService">The configuration service for retrieving business rule parameters.</param>
        /// <param name="unitOfWork">The unit of work instance for accessing repository operations.</param>
        /// <param name="domainHelperService">The helper service for domain hierarchy operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        public BookHelperService(
            IConfigurationSettingService configurationService,
            IUnitOfWork unitOfWork,
            IDomainHelperService domainHelperService)
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _domainHelperService = domainHelperService ?? throw new ArgumentNullException(nameof(domainHelperService));
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="AggregateValidationException"></exception>
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

        /// <inheritdoc/>
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
