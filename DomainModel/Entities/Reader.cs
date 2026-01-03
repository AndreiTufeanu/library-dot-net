using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    /// <summary>Represents a library reader.</summary>
    public class Reader
    {
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Gets or sets the first name.</summary>
        /// <value>The first name.</value>
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Fist Name must have between {2} and {1} characters.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "First Name can only contain letters and spaces.")]
        public string FirstName { get; set; }

        /// <summary>Gets or sets the last name.</summary>
        /// <value>The last name.</value>
        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Last Name must have between {2} and {1} characters.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Last Name can only contain letters and spaces.")]
        public string LastName { get; set; }

        /// <summary>Gets or sets the address.</summary>
        /// <value>The address.</value>
        [Required(ErrorMessage = "Address is required.")]
        [StringLength(300, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 300 characters.")]
        public string Address { get; set; }

        /// <summary>Gets or sets the date of birth.</summary>
        /// <value>The date of birth.</value>
        [Required(ErrorMessage = "Date of birth is required.")]
        public DateTime DateOfBirth { get; set; }

        /// <summary>Gets or sets the phone number.</summary>
        /// <value>The phone number.</value>
        [StringLength(20, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 20 characters.")]
        [RegularExpression(@"^[\+]?[0-9\s\-\(\)]+$", ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; }

        /// <summary>Gets or sets the email.</summary>
        /// <value>The email.</value>
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        /// <summary>Gets or sets the librarian account.</summary>
        /// <value>The librarian account.</value>
        public virtual Librarian LibrarianAccount { get; set; }

        /// <summary>Gets or sets the borrowings.</summary>
        /// <value>The borrowings.</value>
        public virtual ICollection<Borrowing> Borrowings { get; set; }
    }
}
