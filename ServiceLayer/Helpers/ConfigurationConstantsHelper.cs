using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Helpers
{
    /// <summary>
    /// Provides centralized configuration constants for the library management system.
    /// Contains all configuration keys and their default values with descriptions.
    /// </summary>
    public static class ConfigurationConstants
    {
        /// <summary>
        /// Maximum number of domains/categories that can be assigned to a single book.
        /// Original requirement constant: DOMENII
        /// </summary>
        public const string MaxDomainsPerBook = "MaxDomainsPerBook";

        /// <summary>
        /// Default value for maximum domains per book.
        /// Original requirement constant: DOMENII
        /// </summary>
        public const int DefaultMaxDomainsPerBook = 5;

        /// <summary>
        /// Maximum number of books a reader can borrow within a period.
        /// Original requirement constant: NMC
        /// </summary>
        public const string MaxBooksInPeriod = "MaxBooksInPeriod";

        /// <summary>
        /// Default value for maximum books in period.
        /// Original requirement constant: NMC
        /// </summary>
        public const int DefaultMaxBooksInPeriod = 10;

        /// <summary>
        /// Window (in days) for MaxBooksInPeriod.
        /// Original requirement constant: PER
        /// </summary>
        public const string MaxBooksInPeriodWindowDays = "MaxBooksInPeriodWindowDays";

        /// <summary>
        /// Default value for MaxBooksInPeriod window.
        /// Original requirement constant: PER
        /// </summary>
        public const int DefaultMaxBooksInPeriodWindowDays = 14;

        /// <summary>
        /// Maximum loan duration for a single borrowing (in days).
        /// </summary>
        public const string BorrowingPeriodDays = "BorrowingPeriodDays";

        /// <summary>
        /// Default value for borrowing period (max loan duration).
        /// </summary>
        public const int DefaultBorrowingPeriodDays = 30;

        /// <summary>
        /// Maximum number of books per single borrowing transaction.
        /// Original requirement constant: C
        /// </summary>
        public const string MaxBooksPerBorrowing = "MaxBooksPerBorrowing";

        /// <summary>
        /// Default value for maximum books per borrowing.
        /// Original requirement constant: C
        /// </summary>
        public const int DefaultMaxBooksPerBorrowing = 5;

        /// <summary>
        /// Maximum number of books from the same domain within the same-domain time window.
        /// Original requirement constant: D
        /// </summary>
        public const string MaxBooksSameDomain = "MaxBooksSameDomain";

        /// <summary>
        /// Default value for maximum books from same domain.
        /// Original requirement constant: D
        /// </summary>
        public const int DefaultMaxBooksSameDomain = 3;

        /// <summary>
        /// Time window in months for checking same-domain books.
        /// Important: Use calendar arithmetic (DateTime.AddMonths) when checking this window.
        /// Original requirement constant: L
        /// </summary>
        public const string SameDomainTimeLimitMonths = "SameDomainTimeLimitMonths";

        /// <summary>
        /// Default value for same-domain time limit.
        /// Original requirement constant: L
        /// </summary>
        public const int DefaultSameDomainTimeLimitMonths = 3;

        /// <summary>
        /// Maximum sum of extension days allowed within the extension window.
        /// Original requirement constant: LIM
        /// </summary>
        public const string MaxOvertimeSumDays = "MaxOvertimeSumDays";

        /// <summary>
        /// Default value for maximum overtime sum.
        /// Original requirement constant: LIM
        /// </summary>
        public const int DefaultMaxOvertimeSumDays = 14;

        /// <summary>
        /// Window in months for counting extensions (e.g., "last 3 months").
        /// </summary>
        public const string ExtensionWindowMonths = "ExtensionWindowMonths";

        /// <summary>
        /// Default value for extension window.
        /// </summary>
        public const int DefaultExtensionWindowMonths = 3;

        /// <summary>
        /// Minimum waiting time (in days) before the same book can be borrowed again.
        /// Original requirement constant: DELTA
        /// </summary>
        public const string SameBookDelayDays = "SameBookDelayDays";

        /// <summary>
        /// Default value for same book delay.
        /// Original requirement constant: DELTA
        /// </summary>
        public const int DefaultSameBookDelayDays = 7;

        /// <summary>
        /// Maximum books a reader can borrow in a single day.
        /// Original requirement constant: NCZ
        /// </summary>
        public const string MaxBooksPerDay = "MaxBooksPerDay";

        /// <summary>
        /// Default value for maximum books per day.
        /// Original requirement constant: NCZ
        /// </summary>
        public const int DefaultMaxBooksPerDay = 10;

        /// <summary>
        /// Maximum books a librarian can lend in a single day.
        /// Original requirement constant: PERSIMP
        /// </summary>
        public const string MaxBooksLentPerDay = "MaxBooksLentPerDay";

        /// <summary>
        /// Default value for maximum books lent per day.
        /// Original requirement constant: PERSIMP
        /// </summary>
        public const int DefaultMaxBooksLentPerDay = 50;

        /// <summary>
        /// Gets all default configuration settings with their metadata.
        /// </summary>
        /// <returns>Collection of configuration settings with key, value, data type, category, and description.</returns>
        public static IEnumerable<(string Key, string Value, string DataType, string Category, string Description)> GetAllDefaults()
        {
            return new List<(string, string, string, string, string)>
            {
                // Book Constants
                (MaxDomainsPerBook, DefaultMaxDomainsPerBook.ToString(), "int", "Books",
                    "Maximum number of domains/categories that can be assigned to a single book (DOMENII)"),

                // Reader Constants
                (MaxBooksInPeriod, DefaultMaxBooksInPeriod.ToString(), "int", "Readers",
                    "Maximum number of books a reader can borrow within the configured period (NMC)"),

                (MaxBooksInPeriodWindowDays, DefaultMaxBooksInPeriodWindowDays.ToString(), "int", "Readers",
                    "Time window in days (PER) used to evaluate MaxBooksInPeriod"),

                (BorrowingPeriodDays, DefaultBorrowingPeriodDays.ToString(), "int", "Readers",
                    "Maximum loan duration for a single borrowing (in days)"),

                (MaxBooksPerBorrowing, DefaultMaxBooksPerBorrowing.ToString(), "int", "Readers",
                    "Maximum number of books that can be borrowed in a single transaction (C)"),

                (MaxBooksSameDomain, DefaultMaxBooksSameDomain.ToString(), "int", "Readers",
                    "Maximum number of books from the same domain (or ancestor/descendant domains) allowed in the same domain window (D)"),

                (SameDomainTimeLimitMonths, DefaultSameDomainTimeLimitMonths.ToString(), "int", "Readers",
                    "Time window expressed in months (L). Use calendar arithmetic (AddMonths) to check this window — per project description '3 months'"),

                (MaxOvertimeSumDays, DefaultMaxOvertimeSumDays.ToString(), "int", "Readers",
                    "Maximum total number of extension days (LIM) a reader can accumulate within the extension window"),

                (ExtensionWindowMonths, DefaultExtensionWindowMonths.ToString(), "int", "Readers",
                    "Window in months used to evaluate MaxOvertimeSumDays (e.g. 'last 3 months')"),

                (SameBookDelayDays, DefaultSameBookDelayDays.ToString(), "int", "Readers",
                    "Minimum waiting period in days (DELTA) before a reader can borrow the same book again"),

                (MaxBooksPerDay, DefaultMaxBooksPerDay.ToString(), "int", "Readers",
                    "Maximum number of books a reader can borrow in a single day (NCZ)"),

                // Librarian Constants
                (MaxBooksLentPerDay, DefaultMaxBooksLentPerDay.ToString(), "int", "Librarians",
                    "Maximum number of books a librarian can lend to readers in a single day (PERSIMP)")
            };
        }
    }
}
