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
        /// <remarks>
        /// This property has a private setter and can only be initialized via the constructor
        /// or the <see cref="SetInitialCopies(int)"/> method. Once set, it cannot be modified.
        /// </remarks>
        [Range(1, int.MaxValue, ErrorMessage = "InitialCopies must be at least {1}.")]
        public int InitialCopies { get; private set; }

        /// <summary>Gets or sets the collection of authors who wrote this book.</summary>
        /// <value>A collection of <see cref="Author"/> entities associated with this book.</value>
        /// <remarks>
        /// This collection is initialized as an empty <see cref="HashSet{Author}"/> and is managed
        /// by Entity Framework for navigation. The setter is protected to allow Entity Framework
        /// to proxy and lazy load the collection.
        /// </remarks>
        public virtual ICollection<Author> Authors { get; protected set; } = new HashSet<Author>();

        /// <summary>Gets or sets the collection of domains this book belongs to.</summary>
        /// <value>A collection of <see cref="Domain"/> entities categorizing this book.</value>
        /// <remarks>
        /// This collection is initialized as an empty <see cref="HashSet{Domain}"/> and is managed
        /// by Entity Framework for navigation. The setter is protected to allow Entity Framework
        /// to proxy and lazy load the collection.
        /// </remarks>
        public virtual ICollection<Domain> Domains { get; protected set; } = new HashSet<Domain>();

        /// <summary>Gets or sets the collection of editions available for this book.</summary>
        /// <value>A collection of <see cref="Edition"/> entities representing different versions of this book.</value>
        /// <remarks>
        /// This collection is initialized as an empty <see cref="HashSet{Edition}"/> and is managed
        /// by Entity Framework for navigation. The setter is protected to allow Entity Framework
        /// to proxy and lazy load the collection.
        /// </remarks>
        public virtual ICollection<Edition> Editions { get; protected set; } = new HashSet<Edition>();

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
        private void SetInitialCopies(int copies)
        {
            if (copies < 1)
                throw new ArgumentOutOfRangeException(nameof(copies), "InitialCopies must be at least 1."); 

            InitialCopies = copies;
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

            var availableCopies = Editions?
                .SelectMany(e => e.BookCopies)
                .Count(bc => bc.IsAvailable && !bc.IsLectureRoomOnly) ?? 0;
            var hasNonLectureCopies = Editions?
                .SelectMany(e => e.BookCopies)
                .Any(bc => !bc.IsLectureRoomOnly) ?? false;

            return hasNonLectureCopies && availableCopies >= (InitialCopies * BorrowableThresholdPercentage);
        }
    }
}