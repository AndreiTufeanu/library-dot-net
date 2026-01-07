using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts
{
    public interface IEditionService : IService<Edition>
    {
        /// <summary>
        /// Gets editions for a specific book
        /// </summary>
        /// <param name="bookId">The book identifier</param>
        /// <returns>A service result containing a collection of editions for the specified book</returns>
        Task<ServiceResult<IEnumerable<Edition>>> GetByBookAsync(Guid bookId);

        /// <summary>
        /// Gets editions of a specific book type
        /// </summary>
        /// <param name="bookTypeId">The book type identifier</param>
        /// <returns>A service result containing a collection of editions of the specified book type</returns>
        Task<ServiceResult<IEnumerable<Edition>>> GetByBookTypeAsync(Guid bookTypeId);

        /// <summary>
        /// Gets editions published in a date range
        /// </summary>
        /// <param name="startDate">The start date of the publication range</param>
        /// <param name="endDate">The end date of the publication range</param>
        /// <returns>A service result containing a collection of editions published within the specified date range</returns>
        Task<ServiceResult<IEnumerable<Edition>>> GetPublishedBetweenAsync(DateTime startDate, DateTime endDate);
    }
}
