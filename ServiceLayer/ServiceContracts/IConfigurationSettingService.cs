using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts
{
    public interface IConfigurationSettingService
    {
        // Book Constants
        Task<int> GetMaxDomainsPerBookAsync();
        Task<int> GetDefaultMaxDomainsPerBookAsync();

        // Reader Constants
        Task<int> GetMaxBooksInPeriodAsync(bool forLibrarian = false);
        Task<int> GetMaxBooksInPeriodWindowDaysAsync();
        Task<TimeSpan> GetBorrowingPeriodAsync(bool forLibrarian = false);
        Task<int> GetMaxBooksPerBorrowingAsync(bool forLibrarian = false);
        Task<int> GetMaxBooksSameDomainAsync(bool forLibrarian = false);
        Task<int> GetSameDomainTimeLimitMonthsAsync();
        Task<int> GetMaxOvertimeSumDaysAsync(bool forLibrarian = false);
        Task<int> GetExtensionWindowMonthsAsync();
        Task<TimeSpan> GetSameBookDelayAsync(bool forLibrarian = false);
        Task<int> GetMaxBooksPerDayAsync();

        // Librarian Constants
        Task<int> GetMaxBooksLentPerDayAsync();

        // Method to refresh cache for a specific key
        Task RefreshSettingAsync(string key);
    }
}
