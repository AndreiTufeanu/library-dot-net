using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.RepositoryContracts
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    namespace DomainModel.RepositoryContracts
    {
        /// <summary>
        /// Generic repository interface with common CRUD operations
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        public interface IRepository<TEntity> where TEntity : class
        {
            /// <summary>
            /// Gets an entity by its identifier
            /// </summary>
            /// <param name="id">The entity identifier</param>
            /// <returns>The entity if found, otherwise null</returns>
            Task<TEntity> GetByIdAsync(Guid id);

            /// <summary>
            /// Gets all entities
            /// </summary>
            /// <returns>A collection of all entities</returns>
            Task<IEnumerable<TEntity>> GetAllAsync();

            /// <summary>
            /// Checks if an entity exists by its identifier
            /// </summary>
            /// <param name="id">The entity identifier</param>
            /// <returns>True if the entity exists, otherwise false</returns>
            Task<bool> ExistsAsync(Guid id);

            /// <summary>
            /// Adds a new entity
            /// </summary>
            /// <param name="entity">The entity to add</param>
            /// <returns>The added entity</returns>
            Task<TEntity> AddAsync(TEntity entity);

            /// <summary>
            /// Updates an existing entity
            /// </summary>
            /// <param name="entity">The entity with updated values</param>
            /// <returns>The updated entity</returns>
            Task<TEntity> UpdateAsync(TEntity entity);

            /// <summary>
            /// Deletes an entity by its identifier
            /// </summary>
            /// <param name="id">The entity identifier</param>
            /// <returns>True if deletion was successful, otherwise false</returns>
            Task<bool> DeleteAsync(Guid id);
        }
    }
}
