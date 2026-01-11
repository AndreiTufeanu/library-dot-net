using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceContracts
{
    public interface IConfigurationSettingService
    {
        #region Book Constants

        /// <summary>
        /// Gets the maximum number of domains allowed per book
        /// </summary>
        /// <returns>The configured maximum domains per book</returns>
        Task<int> GetMaxDomainsPerBookAsync();

        /// <summary>
        /// Gets the default maximum number of domains per book
        /// </summary>
        /// <returns>The default maximum domains per book</returns>
        Task<int> GetDefaultMaxDomainsPerBookAsync();

        #endregion

        #region Reader Constants

        /// <summary>
        /// Gets the maximum number of books a reader can borrow within a period
        /// </summary>
        /// <param name="forLibrarian">Whether to apply librarian privileges (doubled limit)</param>
        /// <returns>The configured maximum books in period</returns>
        Task<int> GetMaxBooksInPeriodAsync(bool forLibrarian = false);

        /// <summary>
        /// Gets the window (in days) for the maximum books in period rule
        /// </summary>
        /// <param name="forLibrarian">Whether to apply librarian privileges (halved window)</param>
        /// <returns>The configured window in days</returns>
        Task<int> GetMaxBooksInPeriodWindowDaysAsync(bool forLibrarian = false);

        /// <summary>
        /// Gets the maximum loan duration for a single borrowing (in days)
        /// </summary>
        /// <returns>The configured borrowing period in days</returns>
        Task<int> GetBorrowingPeriodDaysAsync();

        /// <summary>
        /// Gets the maximum number of books per single borrowing transaction
        /// </summary>
        /// <param name="forLibrarian">Whether to apply librarian privileges (doubled limit)</param>
        /// <returns>The configured maximum books per borrowing</returns>
        Task<int> GetMaxBooksPerBorrowingAsync(bool forLibrarian = false);

        /// <summary>
        /// Gets the maximum number of books from the same domain within the same-domain time window
        /// </summary>
        /// <param name="forLibrarian">Whether to apply librarian privileges (doubled limit)</param>
        /// <returns>The configured maximum books from same domain</returns>
        Task<int> GetMaxBooksSameDomainAsync(bool forLibrarian = false);

        /// <summary>
        /// Gets the time window in months for checking same-domain books
        /// </summary>
        /// <returns>The configured same domain time limit in months</returns>
        Task<int> GetSameDomainTimeLimitMonthsAsync();

        /// <summary>
        /// Gets the maximum sum of extension days allowed within the extension window
        /// </summary>
        /// <param name="forLibrarian">Whether to apply librarian privileges (doubled limit)</param>
        /// <returns>The configured maximum overtime sum days</returns>
        Task<int> GetMaxOvertimeSumDaysAsync(bool forLibrarian = false);

        /// <summary>
        /// Gets the window in months for counting extensions
        /// </summary>
        /// <returns>The configured extension window in months</returns>
        Task<int> GetExtensionWindowMonthsAsync();

        /// <summary>
        /// Gets the minimum waiting time (in days) before the same book can be borrowed again
        /// </summary>
        /// <param name="forLibrarian">Whether to apply librarian privileges (halved delay)</param>
        /// <returns>The configured same book delay in days</returns>
        Task<int> GetSameBookDelayDaysAsync(bool forLibrarian = false);

        /// <summary>
        /// Gets the maximum books a reader can borrow in a single day
        /// </summary>
        /// <returns>The configured maximum books per day</returns>
        Task<int> GetMaxBooksPerDayAsync();

        #endregion

        #region Librarian Constants

        /// <summary>
        /// Gets the maximum books a librarian can lend in a single day
        /// </summary>
        /// <returns>The configured maximum books lent per day</returns>
        Task<int> GetMaxBooksLentPerDayAsync();

        #endregion

        #region Cache Management

        /// <summary>
        /// Refreshes the cached value for a specific configuration key
        /// </summary>
        /// <param name="key">The configuration key to refresh</param>
        Task RefreshSettingAsync(string key);

        #endregion
    }
}
