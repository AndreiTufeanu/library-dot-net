using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Entities
{
    /// <summary>Represents a physical book copy.</summary>
    public class BookCopy
    {
        /// <summary>Gets or sets the unique identifier.</summary>
        /// <value>A unique GUID that identifies this physical book copy in the system.</value>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Gets or sets a value indicating whether this copy is restricted to lecture room use only.</summary>
        /// <value>Returns <c>true</c> if this book copy can only be used within the lecture room; otherwise, <c>false</c>.</value>
        [Required]
        public bool IsLectureRoomOnly { get; set; }

        /// <summary>Gets or sets a value indicating whether this copy is currently available for borrowing.</summary>
        /// <value>Returns <c>true</c> if this book copy is available for borrowing; otherwise, <c>false</c>.</value>
        [Required]
        public bool IsAvailable { get; set; } = true;

        /// <summary>Gets or sets the edition that this physical copy belongs to.</summary>
        /// <value>The <see cref="Entities.Edition"/> that this physical book copy represents.</value>
        [Required(ErrorMessage = "Edition is required.")]
        public virtual Edition Edition { get; set; }

        /// <summary>Gets or sets the collection of borrowing records for this book copy.</summary>
        /// <value>A collection of <see cref="Borrowing"/> entities tracking the loan history of this book copy.</value>
        public virtual ICollection<Borrowing> Borrowings { get; set; }
    }
}