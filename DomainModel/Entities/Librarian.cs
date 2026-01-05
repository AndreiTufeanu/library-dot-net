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
        public virtual ICollection<Borrowing> ProcessedLoans { get; protected set; } = new HashSet<Borrowing>();

        /// <summary>
        /// Determines whether this librarian is also registered as a reader in the system.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the librarian has associated reader details; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This status affects which borrowing limits apply to this user.
        /// When a librarian is also a reader, they benefit from special borrowing privileges
        /// as specified in the business requirements.
        /// </remarks>
        public bool IsAlsoReader()
        {
            return ReaderDetails != null;
        }
    }
}