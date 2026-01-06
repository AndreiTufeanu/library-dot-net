using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts
{
    public interface IReaderService : IService<Reader>
    {
        Task<ServiceResult<Reader>> FindByEmailAsync(string email);
        Task<ServiceResult<Reader>> FindByPhoneAsync(string phoneNumber);
        Task<ServiceResult<IEnumerable<Reader>>> FindByLastNameAsync(string lastName);
        Task<ServiceResult<bool>> HasActiveBorrowingsAsync(Guid readerId);
    }
}
