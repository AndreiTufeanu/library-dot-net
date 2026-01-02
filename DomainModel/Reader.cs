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
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        /// <summary>Gets or sets the last name.</summary>
        /// <value>The last name.</value>
        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        /// <summary>Gets or sets the address.</summary>
        /// <value>The address.</value>
        [Required]
        [StringLength(300)]
        public string Address { get; set; }

        /// <summary>Gets or sets the date of birth.</summary>
        /// <value>The date of birth.</value>
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        /// <summary>Gets or sets the phone number.</summary>
        /// <value>The phone number.</value>
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        /// <summary>Gets or sets the email.</summary>
        /// <value>The email.</value>
        [StringLength(200)]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>Gets or sets the librarian account.</summary>
        /// <value>The librarian account.</value>
        public virtual Librarian LibrarianAccount { get; set; }

        /// <summary>Gets or sets the borrowings.</summary>
        /// <value>The borrowings.</value>
        public virtual ICollection<Borrowing> Borrowings { get; set; }
    }
}
