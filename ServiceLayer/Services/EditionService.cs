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
    /// <summary>Provides service operations for managing <see cref="Edition"/> entities in the library management system.</summary>
    /// <remarks>
    /// Implements <see cref="IEditionService"/> to handle different versions of books.
    /// Enforces publication date validation and prevents deletion of editions with physical copies.
    /// </remarks>
    public class EditionService : BaseService, IEditionService
    {
        /// <summary>The unit of work instance for coordinating repository operations and transactions.</summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>The validator instance for enforcing edition-specific business rules.</summary>
        private readonly IValidator<Edition> _validator;

        /// <summary>Initializes a new instance of the <see cref="EditionService"/> class.</summary>
        /// <param name="unitOfWork">The unit of work instance for coordinating repository operations.</param>
        /// <param name="validator">The validator instance for enforcing edition-specific business rules.</param>
        /// <param name="logger">The logger instance for logging service operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="unitOfWork"/> or <paramref name="validator"/> is null.</exception>
        public EditionService(
            IUnitOfWork unitOfWork,
            IValidator<Edition> validator,
            ILogger<EditionService> logger)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        public async Task<ServiceResult<Edition>> CreateAsync(Edition edition)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                ValidationHelper.Validate(edition, _validator);

                if (!await _unitOfWork.Books.ExistsAsync(edition.Book.Id))
                {
                    throw new NotFoundException(nameof(Book), edition.Book?.Id);
                }

                if (!await _unitOfWork.BookTypes.ExistsAsync(edition.BookType.Id))
                {
                    throw new NotFoundException(nameof(BookType), edition.BookType?.Id);
                }

                var addedEdition = await _unitOfWork.Editions.AddAsync(edition);
                await _unitOfWork.SaveChangesAsync();

                return addedEdition;

            }, nameof(CreateAsync));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        public async Task<ServiceResult<Edition>> GetByIdAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var edition = await _unitOfWork.Editions.GetByIdAsync(id);
                if (edition == null)
                {
                    throw new NotFoundException(nameof(Edition), id);
                }
                return edition;

            }, nameof(GetByIdAsync));
        }

        /// <inheritdoc/>
        public async Task<ServiceResult<IEnumerable<Edition>>> GetAllAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Editions.GetAllAsync();

            }, nameof(GetAllAsync));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        public async Task<ServiceResult<bool>> UpdateAsync(Edition edition)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                if (!await _unitOfWork.Editions.ExistsAsync(edition.Id))
                {
                    throw new NotFoundException(nameof(Edition), edition.Id);
                }

                ValidationHelper.Validate(edition, _validator);

                if (edition.Book != null && !await _unitOfWork.Books.ExistsAsync(edition.Book.Id))
                {
                    throw new NotFoundException(nameof(Book), edition.Book.Id);
                }

                if (edition.BookType != null && !await _unitOfWork.BookTypes.ExistsAsync(edition.BookType.Id))
                {
                    throw new NotFoundException(nameof(BookType), edition.BookType.Id);
                }

                await _unitOfWork.Editions.UpdateAsync(edition);
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
                var edition = await _unitOfWork.Editions.GetByIdAsync(id);
                if (edition == null)
                {
                    throw new NotFoundException(nameof(Edition), id);
                }

                if (await _unitOfWork.Editions.HasCopiesAsync(id))
                {
                    throw new BusinessRuleException("Cannot delete an edition that has physical copies.");
                }

                var success = await _unitOfWork.Editions.DeleteAsync(id);
                if (!success)
                {
                    throw new InvalidOperationException("Failed to delete edition");
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
                return await _unitOfWork.Editions.ExistsAsync(id);

            }, nameof(ExistsAsync));
        }
    }
}
