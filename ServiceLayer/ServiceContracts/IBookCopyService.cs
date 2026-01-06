using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts
{
    public interface IBookCopyService : IService<BookCopy>
    {
        Task<ServiceResult<IEnumerable<BookCopy>>> GetBorrowableCopiesByBookAsync(Guid bookId);
        Task<ServiceResult<IEnumerable<BookCopy>>> GetBorrowableCopiesByEditionAsync(Guid editionId);
        Task<ServiceResult<IEnumerable<BookCopy>>> GetByBookAsync(Guid bookId);
        Task<ServiceResult<IEnumerable<BookCopy>>> GetByEditionAsync(Guid editionId);
        Task<ServiceResult<IEnumerable<BookCopy>>> GetAvailableCopiesAsync();
        Task<ServiceResult<IEnumerable<BookCopy>>> GetLectureRoomOnlyCopiesAsync();
        Task<ServiceResult<bool>> MarkAsBorrowedAsync(Guid bookCopyId);
        Task<ServiceResult<bool>> MarkAsReturnedAsync(Guid bookCopyId);
        Task<ServiceResult<bool>> IsBorrowableAsync(Guid bookCopyId);
    }
}
