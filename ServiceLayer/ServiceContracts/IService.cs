using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts
{
    public interface IService<TEntity> where TEntity : class
    {
        Task<ServiceResult<TEntity>> CreateAsync(TEntity entity);
        Task<ServiceResult<TEntity>> GetByIdAsync(Guid id);
        Task<ServiceResult<IEnumerable<TEntity>>> GetAllAsync();
        Task<ServiceResult<bool>> UpdateAsync(TEntity entity);
        Task<ServiceResult<bool>> DeleteAsync(Guid id);
        Task<ServiceResult<bool>> ExistsAsync(Guid id);
    }

    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();

        public static ServiceResult<T> SuccessResult(T data) => new ServiceResult<T>
        {
            Success = true,
            Data = data
        };

        public static ServiceResult<T> FailureResult(string error) => new ServiceResult<T>
        {
            Success = false,
            ErrorMessage = error
        };

        public static ServiceResult<T> ValidationFailed(List<string> errors) => new ServiceResult<T>
        {
            Success = false,
            ValidationErrors = errors
        };
    }
}
