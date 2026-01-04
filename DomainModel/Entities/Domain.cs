using DomainModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public virtual ICollection<Domain> Subdomains { get; set; }

        /// <summary>Gets or sets the books belonging to this domain.</summary>
        /// <value>A collection of <see cref="Book"/> entities categorized under this domain.</value>
        public virtual ICollection<Book> Books { get; set; }
    }
}