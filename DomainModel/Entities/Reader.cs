using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Entities
{
    /// <summary>Represents a library reader.</summary>
    public class Reader
    {
        /// <summary>Gets or sets the unique identifier.</summary>
        /// <value>A unique GUID that identifies this reader in the system.</value>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Gets or sets the first name.</summary>
        /// <value>The reader's first name.</value>
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Fist Name must have between {2} and {1} characters.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "First Name can only contain letters and spaces.")]
        public string FirstName { get; set; }

        /// <summary>Gets or sets the last name.</summary>
        /// <value>The reader's last name.</value>
        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Last Name must have between {2} and {1} characters.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Last Name can only contain letters and spaces.")]
        public string LastName { get; set; }

        /// <summary>Gets or sets the address.</summary>
        /// <value>The reader's residential address.</value>
        [Required(ErrorMessage = "Address is required.")]
        [StringLength(300, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 300 characters.")]
        public string Address { get; set; }

        /// <summary>Gets or sets the date of birth.</summary>
        /// <value>The reader's birth date.</value>
        [Required(ErrorMessage = "Date of birth is required.")]
        public DateTime DateOfBirth { get; set; }

        /// <summary>Gets or sets the phone number.</summary>
        /// <value>The reader's phone number.</value>
        [StringLength(20, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 20 characters.")]
        [RegularExpression(@"^[\+]?[0-9\s\-\(\)]+$", ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; }

        /// <summary>Gets or sets the email.</summary>
        /// <value>The reader's email address.</value>
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        /// <summary>Gets or sets the librarian account associated with this reader.</summary>
        /// <value>The <see cref="Librarian"/> entity if this reader is also a librarian; otherwise, <c>null</c>.</value>
        public virtual Librarian LibrarianAccount { get; set; }

        /// <summary>Gets or sets the collection of borrowing records for this reader.</summary>
        /// <value>A collection of <see cref="Borrowing"/> entities representing this reader's borrowing history.</value>
        public virtual ICollection<Borrowing> Borrowings { get; set; }
    }
}