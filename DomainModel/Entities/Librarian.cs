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
        /// <summary>Gets or sets the unique identifier.</summary>
        /// <value>A unique GUID that identifies this librarian in the system.</value>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Gets or sets the reader account details for this librarian.</summary>
        /// <value>The <see cref="Reader"/> entity containing the personal details of this librarian.</value>
        public virtual Reader ReaderDetails { get; set; }

        /// <summary>Gets or sets the collection of loans processed by this librarian.</summary>
        /// <value>A collection of <see cref="Borrowing"/> entities that were processed by this librarian.</value>
        /// <remarks>
        /// This collection is initialized as an empty <see cref="HashSet{Borrowing}"/> and is managed
        /// by Entity Framework for navigation. The setter is protected to allow Entity Framework
        /// to proxy and lazy load the collection.
        /// </remarks>
        public virtual ICollection<Borrowing> ProcessedLoans { get; protected set; } = new HashSet<Borrowing>();
    }
}