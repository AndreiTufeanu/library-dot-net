using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Entities
{
    /// <summary>Represents a book edition.</summary>
    public class Edition
    {
        /// <summary>Gets or sets the unique identifier.</summary>
        /// <value>A unique GUID that identifies this edition in the system.</value>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Gets or sets the number of pages.</summary>
        /// <value>The total number of pages in this edition.</value>
        [Required(ErrorMessage = "Number of pages is required.")]
        [Range(30, 5000, ErrorMessage = "Number of pages must be at least {1} and {2}.")]
        public int NumberOfPages { get; set; }

        /// <summary>Gets or sets the publication date.</summary>
        /// <value>The date when this edition was published.</value>
        [Required(ErrorMessage = "Publication date is required.")]
        public DateTime PublicationDate { get; set; }

        /// <summary>Gets or sets the book that this edition belongs to.</summary>
        /// <value>The <see cref="Entities.Book"/> entity that this edition is a version of.</value>
        [Required(ErrorMessage = "Book is required.")]
        public virtual Book Book { get; set; }

        /// <summary>Gets or sets the book type of this edition.</summary>
        /// <value>The <see cref="Entities.BookType"/> (e.g., "Hardcover", "Paperback") of this edition.</value>
        [Required(ErrorMessage = "Book type is required.")]
        public virtual BookType BookType { get; set; }

        /// <summary>Gets or sets the physical copies of this edition.</summary>
        /// <value>A collection of <see cref="BookCopy"/> entities representing the physical copies of this edition.</value>
        public virtual ICollection<BookCopy> BookCopies { get; set; } = new HashSet<BookCopy>();
    }
}