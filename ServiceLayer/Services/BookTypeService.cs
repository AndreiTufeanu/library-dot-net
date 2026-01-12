using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using ServiceLayer.Exceptions;
using ServiceLayer.Helpers;
using ServiceLayer.ServiceContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    /// <summary>Provides service operations for managing <see cref="BookType"/> entities in the library management system.</summary>
    /// <remarks>
    /// Implements <see cref="IBookTypeService"/> to handle book type definitions with uniqueness enforcement.
    /// Manages reference data for book types and prevents deletion of types in use by existing editions.
    /// </remarks>
    public class BookTypeService : BaseService, IBookTypeService
    {
        /// <summary>The unit of work instance for coordinating repository operations and transactions.</summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>Initializes a new instance of the <see cref="BookTypeService"/> class.</summary>
        /// <param name="unitOfWork">The unit of work instance for coordinating repository operations.</param>
        /// <param name="logger">The logger instance for logging service operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="unitOfWork"/> is null.</exception>
        public BookTypeService(
            IUnitOfWork unitOfWork,
            ILogger<BookTypeService> logger)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <inheritdoc/>
        /// <exception cref="AggregateValidationException"></exception>
        public async Task<ServiceResult<BookType>> CreateAsync(BookType bookType)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                ValidationHelper.Validate(bookType);

                var existingBookType = await _unitOfWork.BookTypes.FindByNameAsync(bookType.Name);
                if (existingBookType != null)
                {
                    throw new AggregateValidationException($"A book type with the name '{bookType.Name}' already exists.");
                }

                var addedBookType = await _unitOfWork.BookTypes.AddAsync(bookType);
                await _unitOfWork.SaveChangesAsync();

                return addedBookType;

            }, nameof(CreateAsync));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        public async Task<ServiceResult<BookType>> GetByIdAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var bookType = await _unitOfWork.BookTypes.GetByIdAsync(id);
                if (bookType == null)
                {
                    throw new NotFoundException(nameof(BookType), id);
                }
                return bookType;

            }, nameof(GetByIdAsync));
        }

        /// <inheritdoc/>
        public async Task<ServiceResult<IEnumerable<BookType>>> GetAllAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.BookTypes.GetAllAsync();

            }, nameof(GetAllAsync));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="AggregateValidationException"></exception>
        public async Task<ServiceResult<bool>> UpdateAsync(BookType bookType)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                if (!await _unitOfWork.BookTypes.ExistsAsync(bookType.Id))
                {
                    throw new NotFoundException(nameof(BookType), bookType.Id);
                }

                ValidationHelper.Validate(bookType);

                var existingBookType = await _unitOfWork.BookTypes.FindByNameAsync(bookType.Name);
                if (existingBookType != null && existingBookType.Id != bookType.Id)
                {
                    throw new AggregateValidationException($"Another book type with the name '{bookType.Name}' already exists.");
                }

                await _unitOfWork.BookTypes.UpdateAsync(bookType);
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
                var bookType = await _unitOfWork.BookTypes.GetByIdAsync(id);
                if (bookType == null)
                {
                    throw new NotFoundException(nameof(BookType), id);
                }

                if (await _unitOfWork.BookTypes.HasEditionsAsync(id))
                {
                    throw new BusinessRuleException("Cannot delete a book type that has associated editions.");
                }

                var success = await _unitOfWork.BookTypes.DeleteAsync(id);
                if (!success)
                {
                    throw new InvalidOperationException("Failed to delete book type");
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
                return await _unitOfWork.BookTypes.ExistsAsync(id);

            }, nameof(ExistsAsync));
        }
    }
}
