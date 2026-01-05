using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.RepositoryContracts
{
    public interface IBookCopyRepository : IRepository<BookCopy>
    {
        /// <summary>
        /// Gets borrowable copies for a specific book
        /// </summary>
        Task<IEnumerable<BookCopy>> GetBorrowableCopiesByBookAsync(Guid bookId);

        /// <summary>
        /// Gets borrowable copies for a specific edition
        /// </summary>
        Task<IEnumerable<BookCopy>> GetBorrowableCopiesByEditionAsync(Guid editionId);

        /// <summary>
        /// Gets all copies for a specific book
        /// </summary>
        Task<IEnumerable<BookCopy>> GetByBookAsync(Guid bookId);

        /// <summary>
        /// Gets all copies for a specific edition
        /// </summary>
        Task<IEnumerable<BookCopy>> GetByEditionAsync(Guid editionId);

        /// <summary>
        /// Gets available (not borrowed) copies
        /// </summary>
        Task<IEnumerable<BookCopy>> GetAvailableCopiesAsync();

        /// <summary>
        /// Gets copies restricted to lecture room only
        /// </summary>
        Task<IEnumerable<BookCopy>> GetLectureRoomOnlyCopiesAsync();

        /// <summary>
        /// Checks if a book copy is currently borrowed
        /// </summary>
        Task<bool> IsCurrentlyBorrowedAsync(Guid bookCopyId);
    }
}
