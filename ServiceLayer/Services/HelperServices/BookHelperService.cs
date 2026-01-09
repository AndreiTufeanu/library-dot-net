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

        public BookHelperService(IConfigurationSettingService configurationService)
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        }

        public async Task ValidateMaxDomainsPerBookAsync(ICollection<DomainModel.Entities.Domain> domains)
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
    }
}
