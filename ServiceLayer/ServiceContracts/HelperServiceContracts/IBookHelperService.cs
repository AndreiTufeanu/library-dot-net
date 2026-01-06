using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts.HelperServiceContracts
{
    public interface IBookHelperService
    {
        Task ValidateMaxDomainsPerBookAsync(ICollection<Domain> domains);
    }
}
