using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using ServiceLayer.Exceptions;
using ServiceLayer.Helpers;
using ServiceLayer.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    public class BookCopyService : BaseService, IBookCopyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BookCopyService(
            IUnitOfWork unitOfWork,
            IValidator<BookCopy> validator,
            ILogger<BookCopyService> logger)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<ServiceResult<BookCopy>> CreateAsync(BookCopy bookCopy)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                ValidationHelper.Validate(bookCopy);

                if (bookCopy.Edition == null || !await _unitOfWork.Editions.ExistsAsync(bookCopy.Edition.Id))
                {
                    throw new NotFoundException("Edition", bookCopy.Edition?.Id);
                }

                var addedBookCopy = await _unitOfWork.BookCopies.AddAsync(bookCopy);
                await _unitOfWork.SaveChangesAsync();

                return addedBookCopy;

            }, nameof(CreateAsync));
        }

        public async Task<ServiceResult<BookCopy>> GetByIdAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var bookCopy = await _unitOfWork.BookCopies.GetByIdAsync(id);
                if (bookCopy == null)
                {
                    throw new NotFoundException(nameof(BookCopy), id);
                }
                return bookCopy;

            }, nameof(GetByIdAsync));
        }

        public async Task<ServiceResult<IEnumerable<BookCopy>>> GetAllAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.BookCopies.GetAllAsync();

            }, nameof(GetAllAsync));
        }

        public async Task<ServiceResult<bool>> UpdateAsync(BookCopy bookCopy)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                if (!await _unitOfWork.BookCopies.ExistsAsync(bookCopy.Id))
                {
                    throw new NotFoundException(nameof(BookCopy), bookCopy.Id);
                }

                ValidationHelper.Validate(bookCopy);

                await _unitOfWork.BookCopies.UpdateAsync(bookCopy);
                await _unitOfWork.SaveChangesAsync();
                return true;

            }, nameof(UpdateAsync));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var bookCopy = await _unitOfWork.BookCopies.GetByIdAsync(id);
                if (bookCopy == null)
                {
                    throw new NotFoundException(nameof(BookCopy), id);
                }

                if (await _unitOfWork.BookCopies.IsCurrentlyBorrowedAsync(id))
                {
                    throw new BusinessRuleException("Cannot delete a book copy that is currently borrowed.");
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var success = await _unitOfWork.BookCopies.DeleteAsync(id);
                    if (!success)
                    {
                        throw new InvalidOperationException("Failed to delete book copy");
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();
                    return true;
                }
                catch
                {
                    await _unitOfWork.RollbackAsync();
                    throw;
                }

            }, nameof(DeleteAsync));
        }

        public async Task<ServiceResult<bool>> ExistsAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.BookCopies.ExistsAsync(id);

            }, nameof(ExistsAsync));
        }

        public async Task<ServiceResult<IEnumerable<BookCopy>>> GetBorrowableCopiesByBookAsync(Guid bookId)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var book = await _unitOfWork.Books.GetByIdAsync(bookId);
                if (book == null)
                {
                    throw new NotFoundException(nameof(Book), bookId);
                }

                return await _unitOfWork.BookCopies.GetBorrowableCopiesByBookAsync(bookId);

            }, nameof(GetBorrowableCopiesByBookAsync));
        }

        public async Task<ServiceResult<IEnumerable<BookCopy>>> GetBorrowableCopiesByEditionAsync(Guid editionId)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var edition = await _unitOfWork.Editions.GetByIdAsync(editionId);
                if (edition == null)
                {
                    throw new NotFoundException(nameof(Edition), editionId);
                }

                return await _unitOfWork.BookCopies.GetBorrowableCopiesByEditionAsync(editionId);

            }, nameof(GetBorrowableCopiesByEditionAsync));
        }

        public async Task<ServiceResult<IEnumerable<BookCopy>>> GetByBookAsync(Guid bookId)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var book = await _unitOfWork.Books.GetByIdAsync(bookId);
                if (book == null)
                {
                    throw new NotFoundException(nameof(Book), bookId);
                }

                return await _unitOfWork.BookCopies.GetByBookAsync(bookId);

            }, nameof(GetByBookAsync));
        }

        public async Task<ServiceResult<IEnumerable<BookCopy>>> GetByEditionAsync(Guid editionId)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var edition = await _unitOfWork.Editions.GetByIdAsync(editionId);
                if (edition == null)
                {
                    throw new NotFoundException(nameof(Edition), editionId);
                }

                return await _unitOfWork.BookCopies.GetByEditionAsync(editionId);

            }, nameof(GetByEditionAsync));
        }

        public async Task<ServiceResult<IEnumerable<BookCopy>>> GetAvailableCopiesAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.BookCopies.GetAvailableCopiesAsync();

            }, nameof(GetAvailableCopiesAsync));
        }

        public async Task<ServiceResult<IEnumerable<BookCopy>>> GetLectureRoomOnlyCopiesAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.BookCopies.GetLectureRoomOnlyCopiesAsync();

            }, nameof(GetLectureRoomOnlyCopiesAsync));
        }

        public async Task<ServiceResult<bool>> MarkAsBorrowedAsync(Guid bookCopyId)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var bookCopy = await _unitOfWork.BookCopies.GetByIdAsync(bookCopyId);
                if (bookCopy == null)
                {
                    throw new NotFoundException(nameof(BookCopy), bookCopyId);
                }

                bookCopy.MarkAsBorrowed();

                await _unitOfWork.SaveChangesAsync();
                return true;

            }, nameof(MarkAsBorrowedAsync));
        }

        public async Task<ServiceResult<bool>> MarkAsReturnedAsync(Guid bookCopyId)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var bookCopy = await _unitOfWork.BookCopies.GetByIdAsync(bookCopyId);
                if (bookCopy == null)
                {
                    throw new NotFoundException(nameof(BookCopy), bookCopyId);
                }

                bookCopy.MarkAsReturned();

                await _unitOfWork.SaveChangesAsync();
                return true;

            }, nameof(MarkAsReturnedAsync));
        }

        public async Task<ServiceResult<bool>> IsBorrowableAsync(Guid bookCopyId)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var bookCopy = await _unitOfWork.BookCopies.GetByIdAsync(bookCopyId);
                if (bookCopy == null)
                {
                    throw new NotFoundException(nameof(BookCopy), bookCopyId);
                }

                return bookCopy.IsBorrowable();

            }, nameof(IsBorrowableAsync));
        }
    }
}
