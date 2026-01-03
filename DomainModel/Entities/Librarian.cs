using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Entities
{
    /// <summary>Represents a librarian.</summary>
    public class Librarian
    {
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Gets or sets the reader account.</summary>
        /// <value>The reader account.</value>
        public virtual Reader ReaderDetails { get; set; }

        /// <summary>Gets or sets the processed loans.</summary>
        /// <value>The processed loans.</value>
        public virtual ICollection<Borrowing> ProcessedLoans { get; set; }
    }
}
