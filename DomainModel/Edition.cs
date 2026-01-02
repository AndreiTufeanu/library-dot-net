using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    /// <summary>Represents a book edition.</summary>
    public class Edition
    {
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Gets or sets the number of pages.</summary>
        /// <value>The number of pages.</value>
        public int NumberOfPages { get; set; }

        /// <summary>Gets or sets the publication date.</summary>
        /// <value>The publication date.</value>
        public DateTime PublicationDate { get; set; }

        /// <summary>Gets or sets the book.</summary>
        /// <value>The book.</value>
        public virtual Book Book { get; set; }

        /// <summary>Gets or sets the book type.</summary>
        /// <value>The book type.</value>
        public virtual BookType BookType { get; set; }

        /// <summary>Gets or sets the book copies.</summary>
        /// <value>The book copies.</value>
        public virtual ICollection<BookCopy> BookCopies { get; set; }
    }
}
