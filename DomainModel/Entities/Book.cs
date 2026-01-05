using DomainModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Entities
{
    /// <summary>Represents a book.</summary>
    public class Book
    {
        /// <summary>
        /// The minimum ratio of available copies to initial copies required
        /// for a book to be considered borrowable.
        /// </summary>
        private const double BorrowableThresholdPercentage = 0.1;

        /// <summary>
        /// Initializes a new instance of the <see cref="Book"/> class.
        /// Protected constructor for Entity Framework materialization and inheritance scenarios.
        /// </summary>
        /// <remarks>
        /// This constructor is required by Entity Framework for materializing objects from the database.
        /// It should not be used directly in application code.
        /// </remarks>
        protected Book() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Book"/> class with the specified initial copies.
        /// </summary>
        /// <param name="initialCopies">The initial number of copies when the book is added to the library.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// </exception>
        public Book(int initialCopies)
        {
            SetInitialCopies(initialCopies);
        }

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

        /// <summary>Gets the initial number of copies when the book was added to the library.</summary>
        /// <value>The initial stock count, which can only be set once when creating the book.</value>
        [Range(1, int.MaxValue, ErrorMessage = "InitialCopies must be at least 1.")]
        public int InitialCopies { get; private set; }

        /// <summary>Gets or sets the collection of authors who wrote this book.</summary>
        /// <value>A collection of <see cref="Author"/> entities associated with this book.</value>
        public virtual ICollection<Author> Authors { get; set; } = new HashSet<Author>();

        /// <summary>Gets or sets the collection of domains this book belongs to.</summary>
        /// <value>A collection of <see cref="Domain"/> entities categorizing this book.</value>
        public virtual ICollection<Domain> Domains { get; set; } = new HashSet<Domain>();

        /// <summary>Gets or sets the collection of editions available for this book.</summary>
        /// <value>A collection of <see cref="Edition"/> entities representing different versions of this book.</value>
        public virtual ICollection<Edition> Editions { get; set; } = new HashSet<Edition>();

        /// <summary>
        /// Sets the initial number of copies for this book.
        /// </summary>
        /// <param name="copies">The number of initial copies.</param>
        /// <exception cref="System.InvalidOperationException">
        /// </exception>
        /// <remarks>
        /// This method can only be called once, typically during object construction.
        /// The <see cref="InitialCopies"/> property cannot be modified after the initial value is set.
        /// </remarks>
        public void SetInitialCopies(int copies)
        {
            if (InitialCopies != 0)
                throw new InvalidOperationException("InitialCopies can only be set once.");

            InitialCopies = copies;
        }

        /// <summary>
        /// Gets the current number of available copies (not borrowed and not lecture-room-only).
        /// </summary>
        /// <returns>The count of available copies for borrowing.</returns>
        /// <remarks>
        /// This is a calculated property that requires database access. 
        /// It counts all book copies across editions that are available and not restricted to lecture room.
        /// </remarks>
        public int GetAvailableCopiesCount()
        {
            return Editions?
                .SelectMany(e => e.BookCopies)
                .Count(bc => bc.IsAvailable && !bc.IsLectureRoomOnly) ?? 0;
        }

        /// <summary>
        /// Gets the current number of physical copies (total across all editions).
        /// </summary>
        /// <returns>The total number of physical copies.</returns>
        public int GetTotalPhysicalCopiesCount()
        {
            return Editions?
                .SelectMany(e => e.BookCopies)
                .Count() ?? 0;
        }

        /// <summary>
        /// Determines whether the book can be borrowed according to business rules.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the book has at least one copy that is not restricted
        /// to the lecture room and the number of available copies is at least
        /// 10% of the initial stock; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAvailableForBorrowing()
        {
            if (InitialCopies == 0) return false;

            var availableCopies = GetAvailableCopiesCount();
            var hasNonLectureCopies = Editions?
                .SelectMany(e => e.BookCopies)
                .Any(bc => !bc.IsLectureRoomOnly) ?? false;

            return hasNonLectureCopies && availableCopies >= (InitialCopies * BorrowableThresholdPercentage);
        }

        /// <summary>
        /// Gets the first available borrowable copy from any edition of this book.
        /// </summary>
        /// <returns>
        /// A <see cref="BookCopy"/> that is available for borrowing, or <c>null</c> 
        /// if no borrowable copies are currently available or if the book is not 
        /// available for borrowing according to business rules.
        /// </returns>
        /// <remarks>
        /// This method first checks if the book is available for borrowing (using 
        /// <see cref="IsAvailableForBorrowing"/>). If the book is borrowable, it 
        /// searches across all editions and their copies to find one that is 
        /// available and not restricted to lecture room use.
        /// </remarks>
        public BookCopy GetAvailableBorrowableCopy()
        {
            if (!IsAvailableForBorrowing())
                return null;

            return Editions?
                .SelectMany(e => e.BookCopies)
                .FirstOrDefault(copy => copy.IsBorrowable());
        }
    }
}