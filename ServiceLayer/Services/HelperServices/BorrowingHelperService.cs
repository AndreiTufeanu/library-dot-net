using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using ServiceLayer.Exceptions;
using ServiceLayer.ServiceContracts;
using ServiceLayer.ServiceContracts.HelperServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services.HelperServices
{
    public class BorrowingHelperService : IBorrowingHelperService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfigurationSettingService _configService;
        private readonly IBookHelperService _bookHelperService;

        public BorrowingHelperService(
            IUnitOfWork unitOfWork,
            IConfigurationSettingService configService,
            IBookHelperService bookHelperService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _bookHelperService = bookHelperService ?? throw new ArgumentNullException(nameof(bookHelperService));
        }

        public async Task<bool> IsReaderAlsoLibrarianAsync(Guid readerId)
        {
            return await _unitOfWork.Librarians.IsReaderAlsoLibrarianAsync(readerId);
        }

        public async Task ValidateMaxBooksPerBorrowingAsync(Borrowing borrowing)
        {
            var isLibrarianReader = await IsReaderAlsoLibrarianAsync(borrowing.Reader.Id);
            var maxBooksPerBorrowing = await _configService.GetMaxBooksPerBorrowingAsync(isLibrarianReader);

            var distinctBookCount = borrowing.BookCopies
                                        .Select(bc => bc.Edition?.Book?.Id)
                                        .Where(id => id.HasValue)
                                        .Distinct()
                                        .Count();

            if (distinctBookCount > maxBooksPerBorrowing)
            {
                throw new AggregateValidationException($"Cannot borrow more than {maxBooksPerBorrowing} books in one transaction.");
            }
        }

        public async Task ValidateMaxBooksInPeriodAsync(Borrowing borrowing)
        {
            var isLibrarianReader = await IsReaderAlsoLibrarianAsync(borrowing.Reader.Id);
            var maxBooksInPeriod = await _configService.GetMaxBooksInPeriodAsync(isLibrarianReader);
            var windowDays = await _configService.GetMaxBooksInPeriodWindowDaysAsync(isLibrarianReader);

            var startDate = borrowing.BorrowDate.AddDays(-windowDays);
            var borrowedCount = await _unitOfWork.Borrowings.GetCountByReaderInPeriodAsync(
                                                                    borrowing.Reader.Id, 
                                                                    startDate, 
                                                                    borrowing.BorrowDate);
            if (borrowedCount > maxBooksInPeriod)
            {
                throw new AggregateValidationException($"Reader has exceeded the maximum number of books ({maxBooksInPeriod}) allowed in the {windowDays}-day period.");
            }
        }

        public async Task ValidateMaxBooksSameDomainAsync(Borrowing borrowing)
        {
            var isLibrarianReader = await IsReaderAlsoLibrarianAsync(borrowing.Reader.Id);
            var maxBooksSameDomain = await _configService.GetMaxBooksSameDomainAsync(isLibrarianReader);
            var timeLimitMonths = await _configService.GetSameDomainTimeLimitMonthsAsync();

            var startDate = borrowing.BorrowDate.AddMonths(-timeLimitMonths);

            var bookIds = new HashSet<Guid>();
            foreach (var copy in borrowing.BookCopies)
            {
                if (copy.Edition?.Book?.Id != null)
                {
                    bookIds.Add(copy.Edition.Book.Id);
                }
            }

            foreach (var bookId in bookIds)
            {
                var allDomainIdsInHierarchy = await _bookHelperService.GetCompleteDomainHierarchyForBookAsync(bookId);

                int borrowedCount = await _unitOfWork.Borrowings.GetCountByReaderAndDomainsInPeriodAsync(
                    borrowing.Reader.Id, allDomainIdsInHierarchy.ToList(), startDate);

                if (borrowedCount > maxBooksSameDomain)
                {
                    var book = await _unitOfWork.Books.GetByIdAsync(bookId);
                    var domainNames = string.Join(", ", book.Domains.Select(d => d.Name));
                    throw new AggregateValidationException(
                        $"Reader has exceeded the maximum number of books ({maxBooksSameDomain}) " +
                        $"allowed from domain hierarchy '{domainNames}' within {timeLimitMonths} months.");
                }
            }
        }

        public async Task ValidateMaxOvertimeSumDaysAsync(Borrowing borrowing)
        {
            if (!borrowing.ExtensionDays.HasValue)
                return;

            var isLibrarianReader = await IsReaderAlsoLibrarianAsync(borrowing.Reader.Id);
            var maxOvertimeSumDays = await _configService.GetMaxOvertimeSumDaysAsync(isLibrarianReader);
            var extensionWindowMonths = await _configService.GetExtensionWindowMonthsAsync();

            var startDate = borrowing.BorrowDate.AddMonths(-extensionWindowMonths);
            var totalExtensions = await _unitOfWork.Borrowings
                .GetTotalExtensionDaysByReaderInPeriodAsync(borrowing.Reader.Id, startDate);


            if (totalExtensions + borrowing.ExtensionDays.Value > maxOvertimeSumDays)
            {
                throw new AggregateValidationException($"Reader would exceed the maximum allowed extension days ({maxOvertimeSumDays}) in the last {extensionWindowMonths} months. Current total: {totalExtensions}, requested: {borrowing.ExtensionDays.Value}.");
            }
        }

        public async Task ValidateSameBookDelayAsync(Borrowing borrowing)
        {
            var isLibrarianReader = await IsReaderAlsoLibrarianAsync(borrowing.Reader.Id);
            var sameBookDelay = await _configService.GetSameBookDelayAsync(isLibrarianReader);

            var bookIds = new HashSet<Guid>();
            foreach (var copy in borrowing.BookCopies)
            {
                if (copy.Edition?.Book?.Id != null)
                {
                    bookIds.Add(copy.Edition.Book.Id);
                }
            }

            foreach (var bookId in bookIds)
            {
                var lastBorrowDate = await _unitOfWork.Borrowings
                    .GetLastBorrowDateForBookByReaderAsync(borrowing.Reader.Id, bookId);

                if (lastBorrowDate.HasValue)
                {
                    var timeSinceLastBorrow = borrowing.BorrowDate - lastBorrowDate.Value;
                    if (timeSinceLastBorrow < sameBookDelay)
                    {
                        var book = await _unitOfWork.Books.GetByIdAsync(bookId);
                        var bookTitle = book?.Title ?? "Unknown Book";
                        throw new AggregateValidationException($"Reader cannot borrow '{bookTitle}' again within {sameBookDelay.TotalDays} days. Last borrowed: {lastBorrowDate.Value:yyyy-MM-dd}.");
                    }
                }
            }
        }

        public async Task ValidateMaxBooksPerDayAsync(Borrowing borrowing)
        {
            if (await IsReaderAlsoLibrarianAsync(borrowing.Reader.Id))
                return;

            var maxBooksPerDay = await _configService.GetMaxBooksPerDayAsync();
            var borrowedToday = await _unitOfWork.Borrowings
                .GetCountByReaderOnDateAsync(borrowing.Reader.Id, borrowing.BorrowDate.Date);

            if (borrowedToday > maxBooksPerDay)
            {
                throw new AggregateValidationException($"Reader has exceeded the maximum number of books ({maxBooksPerDay}) allowed per day.");
            }
        }

        public async Task ValidateLibrarianLendingLimitAsync(Borrowing borrowing)
        {
            var maxBooksLentPerDay = await _configService.GetMaxBooksLentPerDayAsync();
            var lentToday = await _unitOfWork.Borrowings.GetCountByLibrarianOnDateAsync(borrowing.Librarian.Id, borrowing.BorrowDate.Date);

            if (lentToday > maxBooksLentPerDay)
            {
                throw new AggregateValidationException($"Librarian has exceeded the maximum number of books ({maxBooksLentPerDay}) allowed to lend per day.");
            }
        }
    }
}
