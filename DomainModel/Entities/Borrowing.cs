using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    /// <summary>Represents a book borrowing.</summary>
    public class Borrowing
    {
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Gets or sets the borrow date.</summary>
        /// <value>The borrow date.</value>
        [Required(ErrorMessage = "Borrow date is required.")]
        public DateTime BorrowDate { get; set; }

        /// <summary>Gets or sets the due date.</summary>
        /// <value>The due date.</value>
        [Required(ErrorMessage = "Due date is required.")]
        public DateTime DueDate { get; set; }

        /// <summary>Gets or sets the return date.</summary>
        /// <value>The return date.</value>
        public DateTime? ReturnDate { get; set; }

        /// <summary>Gets or sets the extended due date.</summary>
        /// <value>The extended due date.</value>
        public DateTime? ExtendedDueDate { get; set; }

        /// <summary>Gets or sets the extension days.</summary>
        /// <value>The extension days.</value>
        public int? ExtensionDays { get; set; }

        /// <summary>Gets or sets the reader.</summary>
        /// <value>The reader.</value>
        [Required(ErrorMessage = "Reader is required.")]
        public virtual Reader Reader { get; set; }

        /// <summary>Gets or sets the borrowed book copies.</summary>
        /// <value>The book copy.</value>
        public virtual ICollection<BookCopy> BookCopies { get; set; }

        /// <summary>Gets or sets the librarian who processed the loan.</summary>
        /// <value>The librarian.</value>
        [Required(ErrorMessage = "Librarian is required.")]
        public virtual Librarian Librarian { get; set; }
    }
}
