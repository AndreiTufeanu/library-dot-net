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
    /// <summary>Provides service operations for managing <see cref="BookCopy"/> entities in the library management system.</summary>
    /// <remarks>
    /// Implements <see cref="IBookCopyService"/> to handle the lifecycle of physical book copies.
    /// Manages state transitions (available → borrowed → returned) and enforces borrowing restrictions.
    /// </remarks>
    public class BookCopyService : BaseService, IBookCopyService
    {
        /// <summary>The unit of work instance for coordinating repository operations and transactions.</summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>Initializes a new instance of the <see cref="BookCopyService"/> class.</summary>
        /// <param name="unitOfWork">The unit of work instance for coordinating repository operations.</param>
        /// <param name="logger">The logger instance for logging service operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="unitOfWork"/> is null.</exception>
        public BookCopyService(
            IUnitOfWork unitOfWork,
            ILogger<BookCopyService> logger)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        public async Task<ServiceResult<BookCopy>> CreateAsync(BookCopy bookCopy)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                ValidationHelper.Validate(bookCopy);

                if (!await _unitOfWork.Editions.ExistsAsync(bookCopy.Edition.Id))
                {
                    throw new NotFoundException("Edition", bookCopy.Edition?.Id);
                }

                var addedBookCopy = await _unitOfWork.BookCopies.AddAsync(bookCopy);
                await _unitOfWork.SaveChangesAsync();

                return addedBookCopy;

            }, nameof(CreateAsync));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
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

        /// <inheritdoc/>
        public async Task<ServiceResult<IEnumerable<BookCopy>>> GetAllAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.BookCopies.GetAllAsync();

            }, nameof(GetAllAsync));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
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

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="BusinessRuleException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
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

                var success = await _unitOfWork.BookCopies.DeleteAsync(id);
                if (!success)
                {
                    throw new InvalidOperationException("Failed to delete book copy");
                }

                await _unitOfWork.SaveChangesAsync();
                return true;

            }, nameof(DeleteAsync));
        }

        /// <inheritdoc/>
        public async Task<ServiceResult<bool>> ExistsAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.BookCopies.ExistsAsync(id);

            }, nameof(ExistsAsync));
        }
    }
}
