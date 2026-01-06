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
        Task<ServiceResult<Domain>> FindByNameAsync(string name);
        Task<ServiceResult<IEnumerable<Domain>>> GetRootDomainsAsync();
        Task<ServiceResult<IEnumerable<Domain>>> GetSubdomainsAsync(Guid parentDomainId);
    }
}
