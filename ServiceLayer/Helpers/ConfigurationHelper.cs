using ServiceLayer.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Helpers
{
    public static class ConfigurationHelper
    {
        private static IConfigurationService _configurationService;

        public static void Initialize(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        // Book Constants
        public static async Task<int> MaxDomainsPerBookAsync()
        {
            CheckInitialized();
            return await _configurationService.GetMaxDomainsPerBookAsync();
        }

        // Reader Constants
        public static async Task<int> MaxBooksInPeriodAsync()
        {
            CheckInitialized();
            return await _configurationService.GetMaxBooksInPeriodAsync();
        }

        public static async Task<TimeSpan> BorrowingPeriodAsync()
        {
            CheckInitialized();
            return await _configurationService.GetBorrowingPeriodAsync();
        }

        public static async Task<int> MaxBooksPerBorrowingAsync()
        {
            CheckInitialized();
            return await _configurationService.GetMaxBooksPerBorrowingAsync();
        }

        public static async Task<int> MaxBooksSameDomainAsync()
        {
            CheckInitialized();
            return await _configurationService.GetMaxBooksSameDomainAsync();
        }

        public static async Task<TimeSpan> SameDomainTimeLimitAsync()
        {
            CheckInitialized();
            return await _configurationService.GetSameDomainTimeLimitAsync();
        }

        public static async Task<int> MaxOvertimeSumAsync()
        {
            CheckInitialized();
            return await _configurationService.GetMaxOvertimeSumAsync();
        }

        public static async Task<TimeSpan> SameBookDelayAsync()
        {
            CheckInitialized();
            return await _configurationService.GetSameBookDelayAsync();
        }

        public static async Task<int> MaxBooksPerDayAsync()
        {
            CheckInitialized();
            return await _configurationService.GetMaxBooksPerDayAsync();
        }

        // Librarian Constants
        public static async Task<int> MaxBooksLentPerDayAsync()
        {
            CheckInitialized();
            return await _configurationService.GetMaxBooksLentPerDayAsync();
        }

        // Librarian-specific adjustments
        public static async Task<int> LibrarianMaxBooksInPeriodAsync()
        {
            CheckInitialized();
            return (await MaxBooksInPeriodAsync()) * 2;
        }

        public static async Task<TimeSpan> LibrarianBorrowingPeriodAsync()
        {
            CheckInitialized();
            var period = await BorrowingPeriodAsync();
            return TimeSpan.FromDays(period.TotalDays / 2);
        }

        public static async Task<int> LibrarianMaxBooksPerBorrowingAsync()
        {
            CheckInitialized();
            return (await MaxBooksPerBorrowingAsync()) * 2;
        }

        public static async Task<int> LibrarianMaxBooksSameDomainAsync()
        {
            CheckInitialized();
            return (await MaxBooksSameDomainAsync()) * 2;
        }

        public static async Task<int> LibrarianMaxOvertimeSumAsync()
        {
            CheckInitialized();
            return (await MaxOvertimeSumAsync()) * 2;
        }

        public static async Task<TimeSpan> LibrarianSameBookDelayAsync()
        {
            CheckInitialized();
            var delay = await SameBookDelayAsync();
            return TimeSpan.FromDays(delay.TotalDays / 2);
        }

        private static void CheckInitialized()
        {
            if (_configurationService == null)
            {
                throw new InvalidOperationException(
                    "ConfigurationHelper must be initialized before use. " +
                    "Call Initialize() in your application startup.");
            }
        }
    }
}
