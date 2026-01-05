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

        /// <summary>Gets a value indicating whether this copy is currently available for borrowing.</summary>
        /// <value>Returns <c>true</c> if this book copy is available for borrowing; otherwise, <c>false</c>.</value>
        [Required]
        public bool IsAvailable { get; private set; } = true;

        /// <summary>Gets or sets the edition that this physical copy belongs to.</summary>
        /// <value>The <see cref="Entities.Edition"/> that this physical book copy represents.</value>
        [Required(ErrorMessage = "Edition is required.")]
        public virtual Edition Edition { get; set; }

        /// <summary>Gets or sets the collection of borrowing records for this book copy.</summary>
        /// <value>A collection of <see cref="Borrowing"/> entities tracking the loan history of this book copy.</value>
        public virtual ICollection<Borrowing> Borrowings { get; protected set; } = new HashSet<Borrowing>();

        /// <summary>
        /// Determines whether this book copy is currently available for borrowing.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the copy is marked as available and is not restricted to lecture room use;
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method checks both the availability flag and lecture room restriction.
        /// A copy must satisfy both conditions to be borrowable.
        /// </remarks>
        public bool IsBorrowable()
        {
            return IsAvailable && !IsLectureRoomOnly;
        }

        /// <summary>
        /// Marks this book copy as borrowed, making it unavailable for other readers.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to borrow a copy that is not currently borrowable.
        /// </exception>
        /// <remarks>
        /// This method should be called when creating a new borrowing transaction.
        /// It ensures the copy is borrowable before changing its status.
        /// </remarks>
        public void MarkAsBorrowed()
        {
            if (!IsBorrowable())
                throw new InvalidOperationException("Cannot borrow a book copy that is not available or is lecture-room-only.");

            IsAvailable = false;
        }

        /// <summary>
        /// Marks this book copy as returned, making it available for future borrowing.
        /// </summary>
        /// <remarks>
        /// This method should be called when a borrowing transaction is completed.
        /// It does not validate the current state, allowing returns even if the copy
        /// was not marked as borrowed (for data consistency recovery).
        /// </remarks>
        public void MarkAsReturned()
        {
            IsAvailable = true;
        }
    }
}