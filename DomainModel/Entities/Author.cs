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
    /// <summary>Represents a book author.</summary>
    public class Author
    {
        /// <summary>Gets or sets the unique identifier.</summary>
        /// <value>A unique GUID that identifies this author in the system.</value>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Gets or sets the first name.</summary>
        /// <value>The author's first name.</value>
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Fist Name must have between {2} and {1} characters.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "First Name can only contain letters and spaces.")]
        public string FirstName { get; set; }

        /// <summary>Gets or sets the last name.</summary>
        /// <value>The author's last name.</value>
        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Last Name must have between {2} and {1} characters.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Last Name can only contain letters and spaces.")]
        public string LastName { get; set; }

        /// <summary>Gets or sets the collection of books written by this author.</summary>
        /// <value>A collection of <see cref="Book"/> entities associated with this author.</value>
        /// <remarks>
        /// This collection is initialized as an empty <see cref="HashSet{Book}"/> and is managed
        /// by Entity Framework for navigation. The setter is protected to allow Entity Framework
        /// to proxy and lazy load the collection.
        /// </remarks>
        public virtual ICollection<Book> Books { get; protected set; } = new HashSet<Book>();
    }
}