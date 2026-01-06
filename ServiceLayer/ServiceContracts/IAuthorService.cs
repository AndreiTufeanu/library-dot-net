using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts
{
    public interface IAuthorService : IService<Author>
    {
        Task<ServiceResult<Author>> FindByNameAsync(string firstName, string lastName);
        Task<ServiceResult<IEnumerable<Author>>> FindByLastNameAsync(string lastName);
    }
}
