using DomainModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMapper
{
    /// <summary>Represents a book author.</summary>
    public class Author
    {
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Gets or sets the first name.</summary>
        /// <value>The first name.</value>
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Fist Name must have between 2 and 100 characters.")]
        public string FirstName { get; set; }

        /// <summary>Gets or sets the last name.</summary>
        /// <value>The last name.</value>
        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Last Name must have between 2 and 100 characters.")]
        public string LastName { get; set; }

        /// <summary>Gets or sets the books.</summary>
        /// <value>The books.</value>
        public virtual ICollection<Book> Books { get; set; }
    }
}
