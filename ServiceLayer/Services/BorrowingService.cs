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
    /// <summary>Provides service operations for managing <see cref="Borrowing"/> entities in the library management system.</summary>
    /// <remarks>
    /// Implements <see cref="IBorrowingService"/> to handle the complete borrowing lifecycle.
    /// Enforces all borrowing constraints with privilege adjustments.
    /// </remarks>

    public class BorrowingService : BaseService, IBorrowingService
    {
        /// <summary>The unit of work instance for coordinating repository operations and transactions.</summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>The validator instance for enforcing borrowing-specific business rules.</summary>
        private readonly IValidator<Borrowing> _validator;

        /// <summary>The helper service for borrowing rule validation.</summary>
        private readonly IBorrowingHelperService _borrowingHelperService;

        /// <summary>The configuration service for retrieving business rule parameters.</summary>
        private readonly IConfigurationSettingService _configService;

        /// <summary>Initializes a new instance of the <see cref="BorrowingService"/> class.</summary>
        /// <param name="unitOfWork">The unit of work instance for coordinating repository operations.</param>
        /// <param name="validator">The validator instance for enforcing borrowing-specific business rules.</param>
        /// <param name="borrowingHelperService">The helper service for borrowing rule validation.</param>
        /// <param name="logger">The logger instance for logging service operations.</param>
        /// <param name="configService">The configuration service for retrieving business rule parameters.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        public BorrowingService(
            IUnitOfWork unitOfWork,
            IValidator<Borrowing> validator,
            IBorrowingHelperService borrowingHelperService,
            ILogger<BorrowingService> logger,
            IConfigurationSettingService configService)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _borrowingHelperService = borrowingHelperService ?? throw new ArgumentNullException(nameof(borrowingHelperService));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="BusinessRuleException"></exception>
        public async Task<ServiceResult<Borrowing>> CreateAsync(Borrowing borrowing)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                ValidationHelper.Validate(borrowing, _validator);

                if (!await _unitOfWork.Readers.ExistsAsync(borrowing.Reader.Id))
                {
                    throw new NotFoundException(nameof(Reader), borrowing.Reader?.Id);
                }

                if (!await _unitOfWork.Librarians.ExistsAsync(borrowing.Librarian.Id))
                {
                    throw new NotFoundException(nameof(Librarian), borrowing.Librarian?.Id);
                }

                foreach (var bookCopy in borrowing.BookCopies)
                {
                    var existingCopy = await _unitOfWork.BookCopies.GetByIdAsync(bookCopy.Id);
                    if (existingCopy == null)
                    {
                        throw new NotFoundException(nameof(BookCopy), bookCopy.Id);
                    }

                    if (!existingCopy.IsBorrowable())
                    {
                        throw new BusinessRuleException($"Book copy {bookCopy.Id} is not available for borrowing.");
                    }
                }

                await _borrowingHelperService.ValidateMaxBooksPerBorrowingAsync(borrowing);
                await _borrowingHelperService.ValidateMaxBooksInPeriodAsync(borrowing);
                await _borrowingHelperService.ValidateMaxBooksSameDomainAsync(borrowing);
                await _borrowingHelperService.ValidateMaxOvertimeSumDaysAsync(borrowing);
                await _borrowingHelperService.ValidateSameBookDelayAsync(borrowing);
                await _borrowingHelperService.ValidateMaxBooksPerDayAsync(borrowing);
                await _borrowingHelperService.ValidateLibrarianLendingLimitAsync(borrowing);

                var borrowingPeriod = await _configService.GetBorrowingPeriodDaysAsync();
                borrowing.DueDate = borrowing.BorrowDate.AddDays(borrowingPeriod);

                foreach (var bookCopy in borrowing.BookCopies)
                {
                    bookCopy.MarkAsBorrowed();
                }

                var addedBorrowing = await _unitOfWork.Borrowings.AddAsync(borrowing);
                await _unitOfWork.SaveChangesAsync();

                return addedBorrowing;

            }, nameof(CreateAsync));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        public async Task<ServiceResult<Borrowing>> GetByIdAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var borrowing = await _unitOfWork.Borrowings.GetByIdAsync(id);
                if (borrowing == null)
                {
                    throw new NotFoundException(nameof(Borrowing), id);
                }
                return borrowing;

            }, nameof(GetByIdAsync));
        }

        /// <inheritdoc/>
        public async Task<ServiceResult<IEnumerable<Borrowing>>> GetAllAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Borrowings.GetAllAsync();

            }, nameof(GetAllAsync));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        public async Task<ServiceResult<bool>> UpdateAsync(Borrowing borrowing)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                if (!await _unitOfWork.Borrowings.ExistsAsync(borrowing.Id))
                {
                    throw new NotFoundException(nameof(Borrowing), borrowing.Id);
                }

                ValidationHelper.Validate(borrowing, _validator);

                await _unitOfWork.Borrowings.UpdateAsync(borrowing);
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
                var borrowing = await _unitOfWork.Borrowings.GetByIdAsync(id);
                if (borrowing == null)
                {
                    throw new NotFoundException(nameof(Borrowing), id);
                }

                if (borrowing.ReturnDate.HasValue)
                {
                    throw new BusinessRuleException("Cannot delete a finished borrowing.");
                }

                var success = await _unitOfWork.Borrowings.DeleteAsync(id);
                if (!success)
                {
                    throw new InvalidOperationException("Failed to delete borrowing");
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
                return await _unitOfWork.Borrowings.ExistsAsync(id);

            }, nameof(ExistsAsync));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        public async Task<ServiceResult<bool>> FinishBorrowingAsync(Guid borrowingId, DateTime? returnDate = null)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var borrowing = await _unitOfWork.Borrowings.GetByIdAsync(borrowingId);
                if (borrowing == null)
                {
                    throw new NotFoundException(nameof(Borrowing), borrowingId);
                }

                borrowing.Finish(returnDate);

                await _unitOfWork.SaveChangesAsync();
                return true;

            }, nameof(FinishBorrowingAsync));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="BusinessRuleException"></exception>
        /// <exception cref="AggregateValidationException"></exception>
        public async Task<ServiceResult<bool>> ExtendBorrowingAsync(Guid borrowingId, int extensionDays)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var borrowing = await _unitOfWork.Borrowings.GetByIdAsync(borrowingId);
                if (borrowing == null)
                {
                    throw new NotFoundException(nameof(Borrowing), borrowingId);
                }

                if (borrowing.ReturnDate.HasValue)
                {
                    throw new BusinessRuleException("Cannot extend a finished borrowing.");
                }

                if (extensionDays <= 0)
                {
                    throw new AggregateValidationException("Extension days must be positive.");
                }

                var testBorrowing = new Borrowing
                {
                    Reader = borrowing.Reader,
                    ExtensionDays = extensionDays,
                    BorrowDate = DateTime.Now
                };

                await _borrowingHelperService.ValidateMaxOvertimeSumDaysAsync(testBorrowing);

                borrowing.ExtensionDays = (borrowing.ExtensionDays ?? 0) + extensionDays;
                borrowing.DueDate = borrowing.DueDate.AddDays(extensionDays);

                await _unitOfWork.SaveChangesAsync();
                return true;

            }, nameof(ExtendBorrowingAsync));
        }
    }
}
