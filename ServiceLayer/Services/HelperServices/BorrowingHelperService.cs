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

        public BorrowingHelperService(
            IUnitOfWork unitOfWork,
            IConfigurationSettingService configService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        }

        public async Task<bool> IsReaderAlsoLibrarianAsync(Guid readerId)
        {
            return await _unitOfWork.Librarians.IsReaderAlsoLibrarianAsync(readerId);
        }

        public async Task<int> GetBooksBorrowedInPeriodAsync(Guid readerId, DateTime startDate, DateTime endDate)
        {
            return await _unitOfWork.Borrowings.GetCountByReaderInPeriodAsync(readerId, startDate, endDate);
        }

        public async Task<int> GetBooksBorrowedFromDomainInPeriodAsync(Guid readerId, Guid domainId, DateTime startDate)
        {
            // Gets books borrowed from a specific domain (does not include hierarchy)
            return await _unitOfWork.Borrowings.GetCountByReaderAndDomainInPeriodAsync(readerId, domainId, startDate);
        }

        public async Task<int> GetTotalExtensionDaysInPeriodAsync(Guid readerId, DateTime startDate)
        {
            return await _unitOfWork.Borrowings.GetTotalExtensionDaysByReaderInPeriodAsync(readerId, startDate);
        }

        public async Task<DateTime?> GetLastBorrowDateForBookAsync(Guid readerId, Guid bookId)
        {
            return await _unitOfWork.Borrowings.GetLastBorrowDateForBookByReaderAsync(readerId, bookId);
        }

        public async Task<int> GetBooksBorrowedOnDateAsync(Guid readerId, DateTime date)
        {
            return await _unitOfWork.Borrowings.GetCountByReaderOnDateAsync(readerId, date);
        }

        public async Task<int> GetBooksLentByLibrarianOnDateAsync(Guid librarianId, DateTime date)
        {
            return await _unitOfWork.Borrowings.GetCountByLibrarianOnDateAsync(librarianId, date);
        }

        public async Task ValidateMaxBooksPerBorrowingAsync(Borrowing borrowing)
        {
            if (borrowing.BookCopies == null)
                return;

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
            if (borrowing.Reader == null)
                return;

            var isLibrarianReader = await IsReaderAlsoLibrarianAsync(borrowing.Reader.Id);
            var maxBooksInPeriod = await _configService.GetMaxBooksInPeriodAsync(isLibrarianReader);
            var windowDays = await _configService.GetMaxBooksInPeriodWindowDaysAsync(isLibrarianReader);

            var startDate = borrowing.BorrowDate.AddDays(-windowDays);
            var borrowedCount = await GetBooksBorrowedInPeriodAsync(borrowing.Reader.Id, startDate, borrowing.BorrowDate);

            if (borrowedCount >= maxBooksInPeriod)
            {
                throw new AggregateValidationException($"Reader has reached the maximum number of books ({maxBooksInPeriod}) allowed in the {windowDays}-day period.");
            }
        }

        public async Task ValidateMaxBooksSameDomainAsync(Borrowing borrowing)
        {
            if (borrowing.Reader == null || borrowing.BookCopies == null)
                return;

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
                var book = await _unitOfWork.Books.GetByIdAsync(bookId);
                if (book?.Domains == null)
                    continue;

                var allDomainIdsInHierarchy = await GetCompleteDomainHierarchyForBookAsync(bookId);

                int borrowedCount = await _unitOfWork.Borrowings.GetCountByReaderAndDomainsInPeriodAsync(
    borrowing.Reader.Id, allDomainIdsInHierarchy.ToList(), startDate);

                if (borrowedCount >= maxBooksSameDomain)
                {
                    var domainNames = string.Join(", ", book.Domains.Select(d => d.Name));
                    throw new AggregateValidationException(
                        $"Reader has reached the maximum number of books ({maxBooksSameDomain}) " +
                        $"allowed from domain hierarchy '{domainNames}' within {timeLimitMonths} months.");
                }
            }
        }

        public async Task ValidateMaxOvertimeSumDaysAsync(Borrowing borrowing)
        {
            if (borrowing.Reader == null || !borrowing.ExtensionDays.HasValue)
                return;

            var isLibrarianReader = await IsReaderAlsoLibrarianAsync(borrowing.Reader.Id);
            var maxOvertimeSumDays = await _configService.GetMaxOvertimeSumDaysAsync(isLibrarianReader);
            var extensionWindowMonths = await _configService.GetExtensionWindowMonthsAsync();

            var startDate = borrowing.BorrowDate.AddMonths(-extensionWindowMonths);
            var totalExtensions = await GetTotalExtensionDaysInPeriodAsync(borrowing.Reader.Id, startDate);

            if (totalExtensions + borrowing.ExtensionDays.Value > maxOvertimeSumDays)
            {
                throw new AggregateValidationException($"Reader would exceed the maximum allowed extension days ({maxOvertimeSumDays}) in the last {extensionWindowMonths} months. Current total: {totalExtensions}, requested: {borrowing.ExtensionDays.Value}.");
            }
        }

        public async Task ValidateSameBookDelayAsync(Borrowing borrowing)
        {
            if (borrowing.Reader == null || borrowing.BookCopies == null)
                return;

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
                var lastBorrowDate = await GetLastBorrowDateForBookAsync(borrowing.Reader.Id, bookId);

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
            if (borrowing.Reader == null)
                return;

            if (await IsReaderAlsoLibrarianAsync(borrowing.Reader.Id))
                return;

            var maxBooksPerDay = await _configService.GetMaxBooksPerDayAsync();
            var borrowedToday = await GetBooksBorrowedOnDateAsync(borrowing.Reader.Id, borrowing.BorrowDate.Date);

            if (borrowedToday >= maxBooksPerDay)
            {
                throw new AggregateValidationException($"Reader has reached the maximum number of books ({maxBooksPerDay}) allowed per day.");
            }
        }

        public async Task ValidateLibrarianLendingLimitAsync(Borrowing borrowing)
        {
            if (borrowing.Librarian == null)
                return;

            var maxBooksLentPerDay = await _configService.GetMaxBooksLentPerDayAsync();
            var lentToday = await GetBooksLentByLibrarianOnDateAsync(borrowing.Librarian.Id, borrowing.BorrowDate.Date);

            if (lentToday >= maxBooksLentPerDay)
            {
                throw new AggregateValidationException($"Librarian has reached the maximum number of books ({maxBooksLentPerDay}) allowed to lend per day.");
            }
        }
        private async Task<HashSet<Guid>> GetCompleteDomainHierarchyForBookAsync(Guid bookId)
        {
            var book = await _unitOfWork.Books.GetByIdAsync(bookId);
            if (book == null || book.Domains == null)
                return new HashSet<Guid>();

            var allDomainIds = new HashSet<Guid>();

            foreach (var domain in book.Domains)
            {
                allDomainIds.Add(domain.Id);

                await AddAncestorDomainIdsAsync(domain.Id, allDomainIds);

                await AddDescendantDomainIdsAsync(domain.Id, allDomainIds);
            }

            return allDomainIds;
        }

        private async Task AddAncestorDomainIdsAsync(Guid domainId, HashSet<Guid> domainIds)
        {
            var domain = await _unitOfWork.Domains.GetByIdAsync(domainId);
            if (domain?.ParentDomain != null)
            {
                domainIds.Add(domain.ParentDomain.Id);
                await AddAncestorDomainIdsAsync(domain.ParentDomain.Id, domainIds);
            }
        }

        private async Task AddDescendantDomainIdsAsync(Guid domainId, HashSet<Guid> domainIds)
        {
            var subdomains = await _unitOfWork.Domains.GetSubdomainsAsync(domainId);
            foreach (var subdomain in subdomains)
            {
                if (!domainIds.Contains(subdomain.Id))
                {
                    domainIds.Add(subdomain.Id);
                    await AddDescendantDomainIdsAsync(subdomain.Id, domainIds);
                }
            }
        }
    }
}
