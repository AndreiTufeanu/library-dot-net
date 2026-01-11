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
    public class BorrowingService : BaseService, IBorrowingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<Borrowing> _validator;
        private readonly IBorrowingHelperService _borrowingHelperService;
        private readonly IConfigurationSettingService _configService;

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

                if (!borrowing.BookCopies.Any())
                {
                    throw new AggregateValidationException("At least one book copy must be borrowed.");
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

                var borrowingPeriod = await _configService.GetBorrowingPeriodAsync();
                borrowing.DueDate = borrowing.BorrowDate.Add(borrowingPeriod);

                foreach (var bookCopy in borrowing.BookCopies)
                {
                    bookCopy.MarkAsBorrowed();
                }

                var addedBorrowing = await _unitOfWork.Borrowings.AddAsync(borrowing);
                await _unitOfWork.SaveChangesAsync();

                return addedBorrowing;

            }, nameof(CreateAsync));
        }

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

        public async Task<ServiceResult<IEnumerable<Borrowing>>> GetAllAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Borrowings.GetAllAsync();

            }, nameof(GetAllAsync));
        }

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

        public async Task<ServiceResult<bool>> ExistsAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Borrowings.ExistsAsync(id);

            }, nameof(ExistsAsync));
        }

        public async Task<ServiceResult<IEnumerable<Borrowing>>> GetActiveByReaderAsync(Guid readerId)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var reader = await _unitOfWork.Readers.GetByIdAsync(readerId);
                if (reader == null)
                {
                    throw new NotFoundException(nameof(Reader), readerId);
                }

                return await _unitOfWork.Borrowings.GetActiveByReaderAsync(readerId);

            }, nameof(GetActiveByReaderAsync));
        }

        public async Task<ServiceResult<IEnumerable<Borrowing>>> GetByReaderAsync(Guid readerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var reader = await _unitOfWork.Readers.GetByIdAsync(readerId);
                if (reader == null)
                {
                    throw new NotFoundException(nameof(Reader), readerId);
                }

                return await _unitOfWork.Borrowings.GetByReaderAsync(readerId, startDate, endDate);

            }, nameof(GetByReaderAsync));
        }

        public async Task<ServiceResult<IEnumerable<Borrowing>>> GetByLibrarianAsync(Guid librarianId, DateTime? startDate = null, DateTime? endDate = null)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var librarian = await _unitOfWork.Librarians.GetByIdAsync(librarianId);
                if (librarian == null)
                {
                    throw new NotFoundException(nameof(Librarian), librarianId);
                }

                return await _unitOfWork.Borrowings.GetByLibrarianAsync(librarianId, startDate, endDate);

            }, nameof(GetByLibrarianAsync));
        }

        public async Task<ServiceResult<IEnumerable<Borrowing>>> GetOverdueAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Borrowings.GetOverdueAsync();

            }, nameof(GetOverdueAsync));
        }

        public async Task<ServiceResult<IEnumerable<Borrowing>>> GetByBookCopyAsync(Guid bookCopyId)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var bookCopy = await _unitOfWork.BookCopies.GetByIdAsync(bookCopyId);
                if (bookCopy == null)
                {
                    throw new NotFoundException(nameof(BookCopy), bookCopyId);
                }

                return await _unitOfWork.Borrowings.GetByBookCopyAsync(bookCopyId);

            }, nameof(GetByBookCopyAsync));
        }

        public async Task<ServiceResult<IEnumerable<Borrowing>>> GetByBookAsync(Guid bookId)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var book = await _unitOfWork.Books.GetByIdAsync(bookId);
                if (book == null)
                {
                    throw new NotFoundException(nameof(Book), bookId);
                }

                return await _unitOfWork.Borrowings.GetByBookAsync(bookId);

            }, nameof(GetByBookAsync));
        }

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
