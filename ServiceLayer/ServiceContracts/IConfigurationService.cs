using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts
{
    public interface IConfigurationService
    {
        // Book Constants
        Task<int> GetMaxDomainsPerBookAsync();
        Task<int> GetDefaultMaxDomainsPerBookAsync();

        // Reader Constants
        Task<int> GetMaxBooksInPeriodAsync();
        Task<TimeSpan> GetBorrowingPeriodAsync();
        Task<int> GetMaxBooksPerBorrowingAsync();
        Task<int> GetMaxBooksSameDomainAsync();
        Task<TimeSpan> GetSameDomainTimeLimitAsync();
        Task<int> GetMaxOvertimeSumAsync();
        Task<TimeSpan> GetSameBookDelayAsync();
        Task<int> GetMaxBooksPerDayAsync();

        // Librarian Constants
        Task<int> GetMaxBooksLentPerDayAsync();

        // Method to refresh cache for a specific key
        Task RefreshSettingAsync(string key);
    }
}
