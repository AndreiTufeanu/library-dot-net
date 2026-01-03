using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    /// <summary>Represents a physical book copy.</summary>
    public class BookCopy
    {
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Gets or sets a value indicating whether is lecture room only.</summary>
        /// <value>The is lecture room only.</value>
        public bool IsLectureRoomOnly { get; set; }

        /// <summary>Gets or sets a value indicating whether is available.</summary>
        /// <value>The is available.</value>
        public bool IsAvailable { get; set; } = true;

        /// <summary>Gets or sets the edition.</summary>
        /// <value>The edition.</value>
        public virtual Edition Edition { get; set; }

        /// <summary>Gets or sets the borrowings.</summary>
        /// <value>The borrowings.</value>
        public virtual ICollection<Borrowing> Borrowings { get; set; }
    }
}
