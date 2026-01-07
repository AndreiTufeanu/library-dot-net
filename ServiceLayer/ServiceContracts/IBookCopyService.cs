using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts
{
    public interface IBookCopyService : IService<BookCopy>
    {
        /// <summary>
        /// Gets borrowable copies for a specific book
        /// </summary>
        /// <param name="bookId">The book identifier</param>
        /// <returns>A service result containing a collection of book copies that are currently borrowable for the specified book</returns>
        Task<ServiceResult<IEnumerable<BookCopy>>> GetBorrowableCopiesByBookAsync(Guid bookId);

        /// <summary>
        /// Gets borrowable copies for a specific edition
        /// </summary>
        /// <param name="editionId">The edition identifier</param>
        /// <returns>A service result containing a collection of book copies that are currently borrowable for the specified edition</returns>
        Task<ServiceResult<IEnumerable<BookCopy>>> GetBorrowableCopiesByEditionAsync(Guid editionId);

        /// <summary>
        /// Gets all copies for a specific book
        /// </summary>
        /// <param name="bookId">The book identifier</param>
        /// <returns>A service result containing a collection of all book copies for the specified book</returns>
        Task<ServiceResult<IEnumerable<BookCopy>>> GetByBookAsync(Guid bookId);

        /// <summary>
        /// Gets all copies for a specific edition
        /// </summary>
        /// <param name="editionId">The edition identifier</param>
        /// <returns>A service result containing a collection of all book copies for the specified edition</returns>
        Task<ServiceResult<IEnumerable<BookCopy>>> GetByEditionAsync(Guid editionId);

        /// <summary>
        /// Gets available (not borrowed) copies
        /// </summary>
        /// <returns>A service result containing a collection of all book copies that are currently available (not borrowed)</returns>
        Task<ServiceResult<IEnumerable<BookCopy>>> GetAvailableCopiesAsync();

        /// <summary>
        /// Gets copies restricted to lecture room only
        /// </summary>
        /// <returns>A service result containing a collection of book copies that are restricted to lecture room use only</returns>
        Task<ServiceResult<IEnumerable<BookCopy>>> GetLectureRoomOnlyCopiesAsync();

        /// <summary>
        /// Marks a book copy as borrowed
        /// </summary>
        /// <param name="bookCopyId">The book copy identifier</param>
        /// <returns>A service result indicating success or failure of the operation</returns>
        Task<ServiceResult<bool>> MarkAsBorrowedAsync(Guid bookCopyId);

        /// <summary>
        /// Marks a book copy as returned
        /// </summary>
        /// <param name="bookCopyId">The book copy identifier</param>
        /// <returns>A service result indicating success or failure of the operation</returns>
        Task<ServiceResult<bool>> MarkAsReturnedAsync(Guid bookCopyId);

        /// <summary>
        /// Checks if a book copy is currently borrowable
        /// </summary>
        /// <param name="bookCopyId">The book copy identifier</param>
        /// <returns>A service result containing true if the book copy is borrowable; otherwise, false</returns>
        Task<ServiceResult<bool>> IsBorrowableAsync(Guid bookCopyId);
    }
}
