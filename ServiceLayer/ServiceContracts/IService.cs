using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts
{
    /// <summary>
    /// Generic service interface with common CRUD operations
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    public interface IService<TEntity> where TEntity : class
    {
        /// <summary>
        /// Creates a new entity
        /// </summary>
        /// <param name="entity">The entity to create</param>
        /// <returns>A service result containing the created entity</returns>
        Task<ServiceResult<TEntity>> CreateAsync(TEntity entity);

        /// <summary>
        /// Gets an entity by its identifier
        /// </summary>
        /// <param name="id">The entity identifier</param>
        /// <returns>A service result containing the entity if found; otherwise, an error</returns>
        Task<ServiceResult<TEntity>> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <returns>A service result containing a collection of all entities</returns>
        Task<ServiceResult<IEnumerable<TEntity>>> GetAllAsync();

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        /// <param name="entity">The entity with updated values</param>
        /// <returns>A service result indicating success or failure of the operation</returns>
        Task<ServiceResult<bool>> UpdateAsync(TEntity entity);

        /// <summary>
        /// Deletes an entity by its identifier
        /// </summary>
        /// <param name="id">The entity identifier</param>
        /// <returns>A service result indicating success or failure of the operation</returns>
        Task<ServiceResult<bool>> DeleteAsync(Guid id);

        /// <summary>
        /// Checks if an entity exists by its identifier
        /// </summary>
        /// <param name="id">The entity identifier</param>
        /// <returns>A service result containing true if the entity exists; otherwise, false</returns>
        Task<ServiceResult<bool>> ExistsAsync(Guid id);
    }

    /// <summary>
    /// Represents the result of a service operation
    /// </summary>
    /// <typeparam name="T">The type of data returned by the operation</typeparam>
    public class ServiceResult<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the data returned by the operation
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Gets or sets the error message if the operation failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the list of validation errors if validation failed
        /// </summary>
        public List<string> ValidationErrors { get; set; } = new List<string>();

        /// <summary>
        /// Creates a successful service result
        /// </summary>
        /// <param name="data">The data to return</param>
        /// <returns>A successful service result</returns>
        public static ServiceResult<T> SuccessResult(T data) => new ServiceResult<T>
        {
            Success = true,
            Data = data
        };

        /// <summary>
        /// Creates a failed service result
        /// </summary>
        /// <param name="error">The error message</param>
        /// <returns>A failed service result</returns>
        public static ServiceResult<T> FailureResult(string error) => new ServiceResult<T>
        {
            Success = false,
            ErrorMessage = error
        };

        /// <summary>
        /// Creates a validation failed service result
        /// </summary>
        /// <param name="errors">The list of validation errors</param>
        /// <returns>A validation failed service result</returns>
        public static ServiceResult<T> ValidationFailed(List<string> errors) => new ServiceResult<T>
        {
            Success = false,
            ValidationErrors = errors
        };
    }
}
