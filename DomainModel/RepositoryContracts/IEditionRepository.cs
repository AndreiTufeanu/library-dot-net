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
        /// Checks if an edition has physical copies
        /// </summary>
        /// <param name="id">The edition identifier</param>
        /// <returns>True if the edition has physical copies; otherwise, false</returns>
        Task<bool> HasCopiesAsync(Guid id);
    }
}
