using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts.HelperServiceContracts
{
    public interface IBorrowingHelperService
    {
        // Validation methods
        Task ValidateMaxBooksPerBorrowingAsync(Borrowing borrowing);
        Task ValidateMaxBooksInPeriodAsync(Borrowing borrowing);
        Task ValidateMaxBooksSameDomainAsync(Borrowing borrowing);
        Task ValidateMaxOvertimeSumDaysAsync(Borrowing borrowing);
        Task ValidateSameBookDelayAsync(Borrowing borrowing);
        Task ValidateMaxBooksPerDayAsync(Borrowing borrowing);
        Task ValidateLibrarianLendingLimitAsync(Borrowing borrowing);

        // Utility methods
        Task<bool> IsReaderAlsoLibrarianAsync(Guid readerId);
        Task<int> GetBooksBorrowedInPeriodAsync(Guid readerId, DateTime startDate, DateTime endDate);
        Task<int> GetBooksBorrowedFromDomainInPeriodAsync(Guid readerId, Guid domainId, DateTime startDate);
        Task<int> GetTotalExtensionDaysInPeriodAsync(Guid readerId, DateTime startDate);
        Task<DateTime?> GetLastBorrowDateForBookAsync(Guid readerId, Guid bookId);
        Task<int> GetBooksBorrowedOnDateAsync(Guid readerId, DateTime date);
        Task<int> GetBooksLentByLibrarianOnDateAsync(Guid librarianId, DateTime date);
    }
}
