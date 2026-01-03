using DataMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    /// <summary>Represents a book.</summary>
    public class Book
    {
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Gets or sets the title.</summary>
        /// <value>The title.</value>
        [StringLength(300)]
        public string Title { get; set; }

        /// <summary>Gets or sets the description.</summary>
        /// <value>The description.</value>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>Gets or sets the authors.</summary>
        /// <value>The authors.</value>
        public virtual ICollection<Author> Authors { get; set; }

        /// <summary>Gets or sets the domains.</summary>
        /// <value>The domains.</value>
        public virtual ICollection<Domain> Domains { get; set; }

        /// <summary>Gets or sets the editions.</summary>
        /// <value>The editions.</value>
        public virtual ICollection<Edition> Editions { get; set; }
    }
}
