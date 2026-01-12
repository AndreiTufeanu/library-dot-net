using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using ServiceLayer.Exceptions;
using ServiceLayer.Helpers;
using ServiceLayer.ServiceContracts;
using ServiceLayer.ServiceContracts.HelperServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    /// <summary>Provides service operations for managing <see cref="Book"/> entities in the library management system.</summary>
    /// <remarks>
    /// Implements <see cref="IBookService"/> to provide CRUD operations with comprehensive business rule validation.
    /// Enforces rules such as maximum domains per book and prevents invalid domain relationships.
    /// </remarks>
    public class BookService : BaseService, IBookService
    {
        /// <summary>The unit of work instance for coordinating repository operations and transactions.</summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>The validator instance for enforcing book-specific business rules.</summary>
        private readonly IValidator<Book> _validator;

        /// <summary>The helper service for book-related validation operations.</summary>
        private readonly IBookHelperService _bookHelperService;

        /// <summary>Initializes a new instance of the <see cref="BookService"/> class.</summary>
        /// <param name="unitOfWork">The unit of work instance for coordinating repository operations.</param>
        /// <param name="validator">The validator instance for enforcing book-specific business rules.</param>
        /// <param name="bookHelperService">The helper service for book-related validation operations.</param>
        /// <param name="logger">The logger instance for logging service operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        public BookService(
            IUnitOfWork unitOfWork,
            IValidator<Book> validator,
            IBookHelperService bookHelperService,
            ILogger<BookService> logger)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _bookHelperService = bookHelperService ?? throw new ArgumentNullException(nameof(bookHelperService));
        }

        /// <inheritdoc/>
        public async Task<ServiceResult<Book>> CreateAsync(Book book)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                ValidationHelper.Validate(book, _validator);
                await _bookHelperService.ValidateMaxDomainsPerBookAsync(book.Domains);

                return await _unitOfWork.Books.AddAsync(book);

            }, nameof(CreateAsync));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        public async Task<ServiceResult<Book>> GetByIdAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var book = await _unitOfWork.Books.GetByIdAsync(id);
                if (book == null)
                {
                    throw new NotFoundException(nameof(Book), id);
                }
                return book;

            }, nameof(GetByIdAsync));
        }

        /// <inheritdoc/>
        public async Task<ServiceResult<IEnumerable<Book>>> GetAllAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Books.GetAllAsync();

            }, nameof(GetAllAsync));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        public async Task<ServiceResult<bool>> UpdateAsync(Book book)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                if (!await _unitOfWork.Books.ExistsAsync(book.Id))
                {
                    throw new NotFoundException(nameof(Book), book.Id);
                }

                ValidationHelper.Validate(book, _validator);
                await _bookHelperService.ValidateMaxDomainsPerBookAsync(book.Domains);

                await _unitOfWork.Books.UpdateAsync(book);
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
                var book = await _unitOfWork.Books.GetByIdAsync(id);
                if (book == null)
                {
                    throw new NotFoundException(nameof(Book), id);
                }

                if (await _unitOfWork.Books.HasPhysicalCopiesAsync(id))
                {
                    throw new BusinessRuleException("Cannot delete a book that has physical copies.");
                }

                var success = await _unitOfWork.Books.DeleteAsync(id);
                if (!success)
                {
                    throw new InvalidOperationException("Failed to delete book");
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
                return await _unitOfWork.Books.ExistsAsync(id);

            }, nameof(ExistsAsync));
        }
    }
}
