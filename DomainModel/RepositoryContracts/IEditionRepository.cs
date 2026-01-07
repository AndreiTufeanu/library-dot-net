using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.RepositoryContracts
{
    public interface IEditionRepository : IRepository<Edition>
    {
        /// <summary>
        /// Gets editions for a specific book
        /// </summary>
        /// <param name="bookId">The book identifier</param>
        /// <returns>A collection of editions for the specified book</returns>
        Task<IEnumerable<Edition>> GetByBookAsync(Guid bookId);

        /// <summary>
        /// Gets editions of a specific book type
        /// </summary>
        /// <param name="bookTypeId">The book type identifier</param>
        /// <returns>A collection of editions of the specified book type</returns>
        Task<IEnumerable<Edition>> GetByBookTypeAsync(Guid bookTypeId);

        /// <summary>
        /// Gets editions published in a date range
        /// </summary>
        /// <param name="startDate">The start date of the publication range</param>
        /// <param name="endDate">The end date of the publication range</param>
        /// <returns>A collection of editions published within the specified date range</returns>
        Task<IEnumerable<Edition>> GetPublishedBetweenAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Checks if an edition has physical copies
        /// </summary>
        /// <param name="id">The edition identifier</param>
        /// <returns>True if the edition has physical copies; otherwise, false</returns>
        Task<bool> HasCopiesAsync(Guid id);
    }
}
