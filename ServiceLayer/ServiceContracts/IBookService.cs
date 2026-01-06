using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts
{
    public interface IBookService : IService<Book>
    {
        Task<ServiceResult<IEnumerable<Book>>> FindByTitleAsync(string title);
        Task<ServiceResult<IEnumerable<Book>>> FindByDomainAsync(Guid domainId);
        Task<ServiceResult<IEnumerable<Book>>> FindByAuthorAsync(Guid authorId);
        Task<ServiceResult<IEnumerable<Book>>> GetAvailableForBorrowingAsync();
    }
}
