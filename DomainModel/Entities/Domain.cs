using DomainModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataMapper
{
    /// <summary>Represents a book domain.</summary>
    public class Domain
    {
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Gets or sets the name.</summary>
        /// <value>The name.</value>
        [Required(ErrorMessage = "Domain name is required.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Domain name must be between {2} and {1} characters.")]
        public string Name { get; set; }

        /// <summary>Gets or sets the description.</summary>
        /// <value>The description.</value>
        [StringLength(1000, ErrorMessage = "Description cannot exceed {1} characters.")]
        public string Description { get; set; }

        /// <summary>Gets or sets the parent domain.</summary>
        /// <value>The parent domain.</value>
        public virtual Domain ParentDomain { get; set; }

        /// <summary>Gets or sets the subdomains.</summary>
        /// <value>The subdomains.</value>
        public virtual ICollection<Domain> Subdomains { get; set; }

        /// <summary>Gets or sets the books.</summary>
        /// <value>The books.</value>
        public virtual ICollection<Book> Books { get; set; }
    }
}
