using DomainModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DomainModel.Entities
{
    /// <summary>Represents a book domain.</summary>
    public class Domain
    {
        /// <summary>Gets or sets the unique identifier.</summary>
        /// <value>A unique GUID that identifies this domain in the system.</value>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Gets or sets the name.</summary>
        /// <value>The name of the domain/category (e.g., "Science Fiction", "History").</value>
        [Required(ErrorMessage = "Domain name is required.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Domain name must be between {2} and {1} characters.")]
        public string Name { get; set; }

        /// <summary>Gets or sets the description.</summary>
        /// <value>An optional description of the domain.</value>
        [StringLength(1000, ErrorMessage = "Description cannot exceed {1} characters.")]
        public string Description { get; set; }

        /// <summary>Gets or sets the parent domain.</summary>
        /// <value>The parent <see cref="Domain"/> if this is a subdomain, or <c>null</c> if this is a top-level domain.</value>
        public virtual Domain ParentDomain { get; set; }

        /// <summary>Gets or sets the collection of subdomains.</summary>
        /// <value>A collection of <see cref="Domain"/> entities that are children of this domain.</value>
        public virtual ICollection<Domain> Subdomains { get; protected set; } = new HashSet<Domain>();

        /// <summary>Gets or sets the books belonging to this domain.</summary>
        /// <value>A collection of <see cref="Book"/> entities categorized under this domain.</value>
        public virtual ICollection<Book> Books { get; protected set; } = new HashSet<Book>();

        /// <summary>
        /// Determines whether the specified domain is a descendant of this domain.
        /// </summary>
        /// <param name="otherDomain">The domain to check for descendant relationship.</param>
        /// <returns>
        /// <c>true</c> if the specified domain is a descendant (child, grandchild, etc.) 
        /// of this domain; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when otherDomain is null.</exception>
        /// <remarks>
        /// This method performs a recursive check through the domain hierarchy.
        /// It assumes that the Subdomains navigation property is properly loaded.
        /// </remarks>
        public bool IsDescendant(Domain otherDomain)
        {
            if (otherDomain == null)
                throw new ArgumentNullException(nameof(otherDomain));

            foreach (var subdomain in Subdomains ?? Enumerable.Empty<Domain>())
            {
                if (subdomain.Id == otherDomain.Id || subdomain.IsDescendant(otherDomain))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified domain is an ancestor of this domain.
        /// </summary>
        /// <param name="otherDomain">The domain to check for ancestor relationship.</param>
        /// <returns>
        /// <c>true</c> if the specified domain is an ancestor (parent, grandparent, etc.) 
        /// of this domain; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when otherDomain is null.</exception>
        /// <remarks>
        /// This method traverses up the domain hierarchy chain.
        /// It assumes that the ParentDomain navigation property is properly loaded.
        /// </remarks>
        public bool IsAncestor(Domain otherDomain)
        {
            if (otherDomain == null)
                throw new ArgumentNullException(nameof(otherDomain));

            var current = ParentDomain;
            while (current != null)
            {
                if (current.Id == otherDomain.Id)
                    return true;
                current = current.ParentDomain;
            }

            return false;
        }
    }
}