using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLayer.Exceptions;

namespace ServiceLayer.ServiceContracts.HelperServiceContracts
{
    /// <summary>
    /// Provides helper methods for book-related operations.
    /// </summary>
    public interface IBookHelperService
    {
        /// <summary>
        /// Validates that a book does not exceed the maximum allowed domains.
        /// </summary>
        /// <param name="domains">The collection of domains assigned to the book.</param>
        /// <exception cref="AggregateValidationException">Thrown when the book exceeds the maximum domains limit.</exception>
        Task ValidateMaxDomainsPerBookAsync(ICollection<Domain> domains);

        /// <summary>
        /// Gets the complete domain hierarchy for a book, including all ancestors and descendants of its assigned domains.
        /// </summary>
        /// <param name="bookId">The book identifier.</param>
        /// <returns>A set containing all domain IDs in the hierarchy of the book's domains.</returns>
        /// <remarks>
        /// This method returns all domain IDs that are in an ancestor or descendant relationship
        /// with any of the domains directly assigned to the book. This is useful for business rules
        /// that need to consider domain hierarchies (e.g., borrowing limits across related domains).
        /// </remarks>
        Task<HashSet<Guid>> GetCompleteDomainHierarchyForBookAsync(Guid bookId);
    }
}
