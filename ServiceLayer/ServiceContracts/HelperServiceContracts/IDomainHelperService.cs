using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts.HelperServiceContracts
{
    /// <summary>
    /// Provides helper methods for domain hierarchy operations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This service encapsulates operations related to domain hierarchies, including
    /// traversing ancestor and descendant relationships. It is designed to be used
    /// by other helper services that need domain hierarchy information.
    /// </para>
    /// </remarks>
    public interface IDomainHelperService
    {
        /// <summary>
        /// Recursively adds all ancestor domain IDs for the specified domain to the collection.
        /// </summary>
        /// <param name="domainId">The domain identifier to start from.</param>
        /// <param name="domainIds">The collection to add ancestor domain IDs to.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method traverses up the domain hierarchy chain, adding all parent,
        /// grandparent, etc. domain IDs to the provided collection.
        /// </remarks>
        Task PopulateAncestorDomainIdsAsync(Guid domainId, HashSet<Guid> domainIds);

        /// <summary>
        /// Recursively adds all descendant domain IDs for the specified domain to the collection.
        /// </summary>
        /// <param name="domainId">The domain identifier to start from.</param>
        /// <param name="domainIds">The collection to add descendant domain IDs to.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method traverses down the domain hierarchy chain, adding all child,
        /// grandchild, etc. domain IDs to the provided collection. It ensures no
        /// duplicates are added.
        /// </remarks>
        Task PopulateDescendantDomainIdsAsync(Guid domainId, HashSet<Guid> domainIds);
    }
}
