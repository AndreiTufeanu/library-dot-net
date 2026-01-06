using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts
{
    public interface IBorrowingService : IService<Borrowing>
    {
        Task<ServiceResult<IEnumerable<Borrowing>>> GetActiveByReaderAsync(Guid readerId);
        Task<ServiceResult<IEnumerable<Borrowing>>> GetByReaderAsync(Guid readerId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ServiceResult<IEnumerable<Borrowing>>> GetByLibrarianAsync(Guid librarianId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ServiceResult<IEnumerable<Borrowing>>> GetOverdueAsync();
        Task<ServiceResult<IEnumerable<Borrowing>>> GetByBookCopyAsync(Guid bookCopyId);
        Task<ServiceResult<IEnumerable<Borrowing>>> GetByBookAsync(Guid bookId);
        Task<ServiceResult<bool>> FinishBorrowingAsync(Guid borrowingId, DateTime? returnDate = null);
        Task<ServiceResult<bool>> ExtendBorrowingAsync(Guid borrowingId, int extensionDays);
    }
}
