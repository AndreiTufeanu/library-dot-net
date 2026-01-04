using DomainModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Entities
{
    /// <summary>Represents a book.</summary>
    public class Book
    {
        /// <summary>Gets or sets the unique identifier.</summary>
        /// <value>A unique GUID that identifies this book in the system.</value>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Gets or sets the title.</summary>
        /// <value>The book's title.</value>
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(300, MinimumLength = 2, ErrorMessage = "Title must be between {2} and {1} characters.")]
        public string Title { get; set; }

        /// <summary>Gets or sets the description.</summary>
        /// <value>An optional description of the book.</value>
        [StringLength(500, ErrorMessage = "Description cannot exceed {1} characters.")]
        public string Description { get; set; }

        /// <summary>Gets or sets the collection of authors who wrote this book.</summary>
        /// <value>A collection of <see cref="Author"/> entities associated with this book.</value>
        public virtual ICollection<Author> Authors { get; set; }

        /// <summary>Gets or sets the collection of domains this book belongs to.</summary>
        /// <value>A collection of <see cref="Domain"/> entities categorizing this book.</value>
        public virtual ICollection<Domain> Domains { get; set; }

        /// <summary>Gets or sets the collection of editions available for this book.</summary>
        /// <value>A collection of <see cref="Edition"/> entities representing different versions of this book.</value>
        public virtual ICollection<Edition> Editions { get; set; }
    }
}