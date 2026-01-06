using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts
{
    public interface IEditionService : IService<Edition>
    {
        Task<ServiceResult<IEnumerable<Edition>>> GetByBookAsync(Guid bookId);
        Task<ServiceResult<IEnumerable<Edition>>> GetByBookTypeAsync(Guid bookTypeId);
        Task<ServiceResult<IEnumerable<Edition>>> GetPublishedBetweenAsync(DateTime startDate, DateTime endDate);
    }
}
