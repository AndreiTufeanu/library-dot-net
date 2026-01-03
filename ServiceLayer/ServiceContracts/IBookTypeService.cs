using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts
{
    public interface IBookTypeService
    {
        ServiceResult<BookType> CreateBookType(BookType bookType);
        ServiceResult<BookType> GetBookTypeById(Guid id);
        ServiceResult<IEnumerable<BookType>> GetAllBookTypes();
        ServiceResult<bool> UpdateBookType(BookType bookType);
        ServiceResult<bool> DeleteBookType(Guid id);
        ServiceResult<bool> BookTypeExists(Guid id);
    }

    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string ErrorMessage { get; set; }

        public static ServiceResult<T> SuccessResult(T data) => new ServiceResult<T> { Success = true, Data = data };
        public static ServiceResult<T> FailureResult(string error) => new ServiceResult<T> { Success = false, ErrorMessage = error };
    }
}
