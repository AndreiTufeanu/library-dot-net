using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts
{
    public interface ILibrarianService : IService<Librarian>
    {
        Task<ServiceResult<Librarian>> GetByReaderIdAsync(Guid readerId);
        Task<ServiceResult<bool>> IsReaderAlsoLibrarianAsync(Guid readerId);
        Task<ServiceResult<Librarian>> CreateFromReaderAsync(Guid readerId);
        Task<ServiceResult<bool>> RemoveLibrarianStatusAsync(Guid librarianId);
    }
}
