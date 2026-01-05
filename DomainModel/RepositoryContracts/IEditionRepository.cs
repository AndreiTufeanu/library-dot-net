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
        Task<IEnumerable<Edition>> GetByBookAsync(Guid bookId);

        /// <summary>
        /// Gets editions of a specific book type
        /// </summary>
        Task<IEnumerable<Edition>> GetByBookTypeAsync(Guid bookTypeId);

        /// <summary>
        /// Gets editions published in a date range
        /// </summary>
        Task<IEnumerable<Edition>> GetPublishedBetweenAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Checks if an edition has physical copies
        /// </summary>
        Task<bool> HasCopiesAsync(Guid id);
    }
}
