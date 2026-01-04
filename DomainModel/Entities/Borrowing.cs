using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Entities
{
    /// <summary>Represents a book borrowing.</summary>
    public class Borrowing
    {
        /// <summary>Gets or sets the unique identifier.</summary>
        /// <value>A unique GUID that identifies this borrowing record in the system.</value>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Gets or sets the borrow date.</summary>
        /// <value>The date and time when the book was borrowed.</value>
        [Required(ErrorMessage = "Borrow date is required.")]
        public DateTime BorrowDate { get; set; }

        /// <summary>Gets or sets the due date.</summary>
        /// <value>The original date when the book should be returned.</value>
        [Required(ErrorMessage = "Due date is required.")]
        public DateTime DueDate { get; set; }

        /// <summary>Gets or sets the return date.</summary>
        /// <value>The date when the book was actually returned, or <c>null</c> if not yet returned.</value>
        public DateTime? ReturnDate { get; set; }

        /// <summary>Gets or sets the extension days.</summary>
        /// <value>The number of days the borrowing period was extended, or <c>null</c> if not extended.</value>
        public int? ExtensionDays { get; set; }

        /// <summary>Gets or sets the reader who borrowed the book.</summary>
        /// <value>The <see cref="Entities.Reader"/> entity representing the person who borrowed the book.</value>
        [Required(ErrorMessage = "Reader is required.")]
        public virtual Reader Reader { get; set; }

        /// <summary>Gets or sets the borrowed book copies.</summary>
        /// <value>A collection of <see cref="BookCopy"/> entities that were borrowed in this transaction.</value>
        public virtual ICollection<BookCopy> BookCopies { get; set; }

        /// <summary>Gets or sets the librarian who processed the loan.</summary>
        /// <value>The <see cref="Entities.Librarian"/> entity who processed this borrowing transaction.</value>
        [Required(ErrorMessage = "Librarian is required.")]
        public virtual Librarian Librarian { get; set; }
    }
}