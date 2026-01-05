using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Entities
{
    /// <summary>Represents a type of book.</summary>
    public class BookType
    {
        /// <summary>Gets or sets the unique identifier.</summary>
        /// <value>A unique GUID that identifies this book type in the system.</value>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Gets or sets the name.</summary>
        /// <value>The name of the book type (e.g., "Paperback", "Hardcover").</value>
        [Required(ErrorMessage = "Book type name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Book type name must be between {2} and {1} characters.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Book type name can only contain letters and spaces.")]
        public string Name { get; set; }

        /// <summary>Gets or sets the collection of editions that use this book type.</summary>
        /// <value>A collection of <see cref="Edition"/> entities that belong to this book type.</value>
        public virtual ICollection<Edition> Editions { get; set; } = new HashSet<Edition>();
    }
}