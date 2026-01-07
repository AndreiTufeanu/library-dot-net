using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts.HelperServiceContracts
{
    public interface IBookHelperService
    {
        /// <summary>
        /// Validates that a book does not exceed the maximum allowed domains
        /// </summary>
        /// <param name="domains">The collection of domains assigned to the book</param>
        /// <exception cref="Exceptions.ValidationException">Thrown when the book exceeds the maximum domains limit</exception>
        Task ValidateMaxDomainsPerBookAsync(ICollection<Domain> domains);
    }
}
