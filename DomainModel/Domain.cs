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
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        /// <summary>Gets or sets the description.</summary>
        /// <value>The description.</value>
        [StringLength(1000)]
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
