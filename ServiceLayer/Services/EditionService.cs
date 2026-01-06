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
    public class EditionService : BaseService, IEditionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<Edition> _validator;

        public EditionService(
            IUnitOfWork unitOfWork,
            IValidator<Edition> validator,
            ILogger<EditionService> logger)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<ServiceResult<Edition>> CreateAsync(Edition edition)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                ValidationHelper.Validate(edition, _validator);

                if (edition.Book == null || !await _unitOfWork.Books.ExistsAsync(edition.Book.Id))
                {
                    throw new NotFoundException(nameof(Book), edition.Book?.Id);
                }

                if (edition.BookType == null || !await _unitOfWork.BookTypes.ExistsAsync(edition.BookType.Id))
                {
                    throw new NotFoundException(nameof(BookType), edition.BookType?.Id);
                }

                var addedEdition = await _unitOfWork.Editions.AddAsync(edition);
                await _unitOfWork.SaveChangesAsync();

                return addedEdition;

            }, nameof(CreateAsync));
        }

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

        public async Task<ServiceResult<IEnumerable<Edition>>> GetAllAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Editions.GetAllAsync();

            }, nameof(GetAllAsync));
        }

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

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var success = await _unitOfWork.Editions.DeleteAsync(id);
                    if (!success)
                    {
                        throw new InvalidOperationException("Failed to delete edition");
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
                return await _unitOfWork.Editions.ExistsAsync(id);

            }, nameof(ExistsAsync));
        }

        public async Task<ServiceResult<IEnumerable<Edition>>> GetByBookAsync(Guid bookId)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var book = await _unitOfWork.Books.GetByIdAsync(bookId);
                if (book == null)
                {
                    throw new NotFoundException(nameof(Book), bookId);
                }

                return await _unitOfWork.Editions.GetByBookAsync(bookId);

            }, nameof(GetByBookAsync));
        }

        public async Task<ServiceResult<IEnumerable<Edition>>> GetByBookTypeAsync(Guid bookTypeId)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var bookType = await _unitOfWork.BookTypes.GetByIdAsync(bookTypeId);
                if (bookType == null)
                {
                    throw new NotFoundException(nameof(BookType), bookTypeId);
                }

                return await _unitOfWork.Editions.GetByBookTypeAsync(bookTypeId);

            }, nameof(GetByBookTypeAsync));
        }

        public async Task<ServiceResult<IEnumerable<Edition>>> GetPublishedBetweenAsync(DateTime startDate, DateTime endDate)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                if (startDate > endDate)
                {
                    throw new Exceptions.ValidationException("Start date cannot be after end date.");
                }

                return await _unitOfWork.Editions.GetPublishedBetweenAsync(startDate, endDate);

            }, nameof(GetPublishedBetweenAsync));
        }
    }
}
