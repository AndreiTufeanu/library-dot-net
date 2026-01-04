using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Helpers
{
    public static class ConfigurationConstants
    {
        // Book Configuration
        public const string MaxDomainsPerBook = "MaxDomainsPerBook";
        public const int DefaultMaxDomainsPerBook = 5;

        // Reader Configuration
        public const string MaxBooksInPeriod = "MaxBooksInPeriod";
        public const int DefaultMaxBooksInPeriod = 10;

        public const string BorrowingPeriodDays = "BorrowingPeriodDays";
        public const double DefaultBorrowingPeriodDays = 30.0;

        public const string MaxBooksPerBorrowing = "MaxBooksPerBorrowing";
        public const int DefaultMaxBooksPerBorrowing = 5;

        public const string MaxBooksSameDomain = "MaxBooksSameDomain";
        public const int DefaultMaxBooksSameDomain = 3;

        public const string SameDomainTimeLimitDays = "SameDomainTimeLimitDays";
        public const double DefaultSameDomainTimeLimitDays = 180.0;

        public const string MaxOvertimeSumDays = "MaxOvertimeSumDays";
        public const int DefaultMaxOvertimeSumDays = 14;

        public const string SameBookDelayDays = "SameBookDelayDays";
        public const double DefaultSameBookDelayDays = 7.0;

        public const string MaxBooksPerDay = "MaxBooksPerDay";
        public const int DefaultMaxBooksPerDay = 10;

        // Librarian Configuration
        public const string MaxBooksLentPerDay = "MaxBooksLentPerDay";
        public const int DefaultMaxBooksLentPerDay = 50;

        // Helper methods to get all constants
        public static IEnumerable<(string Key, string Value, string DataType, string Category, string Description)> GetAllDefaults()
        {
            return new List<(string, string, string, string, string)>
            {
                // Book Constants
                (MaxDomainsPerBook, DefaultMaxDomainsPerBook.ToString(), "int", "Books",
                    "Maximum number of domains/categories that can be assigned to a single book"),
                
                // Reader Constants
                (MaxBooksInPeriod, DefaultMaxBooksInPeriod.ToString(), "int", "Readers",
                    "Maximum number of books a reader can borrow within the borrowing period"),

                (BorrowingPeriodDays, DefaultBorrowingPeriodDays.ToString(), "double", "Readers",
                    "Standard borrowing period in days (used in conjunction with MaxBooksInPeriod)"),

                (MaxBooksPerBorrowing, DefaultMaxBooksPerBorrowing.ToString(), "int", "Readers",
                    "Maximum number of books that can be borrowed in a single transaction"),

                (MaxBooksSameDomain, DefaultMaxBooksSameDomain.ToString(), "int", "Readers",
                    "Maximum number of books from the same domain (or ancestor/descendant domains) that can be borrowed within the time limit"),

                (SameDomainTimeLimitDays, DefaultSameDomainTimeLimitDays.ToString(), "double", "Readers",
                    "Time limit in days for monitoring books from the same domain (used with MaxBooksSameDomain)"),

                (MaxOvertimeSumDays, DefaultMaxOvertimeSumDays.ToString(), "int", "Readers",
                    "Maximum total overtime (in days) a reader can accumulate over the past 3 months"),

                (SameBookDelayDays, DefaultSameBookDelayDays.ToString(), "double", "Readers",
                    "Minimum waiting period in days before a reader can borrow the same book again"),

                (MaxBooksPerDay, DefaultMaxBooksPerDay.ToString(), "int", "Readers",
                    "Maximum number of books a reader can borrow in a single day"),
                
                // Librarian Constants
                (MaxBooksLentPerDay, DefaultMaxBooksLentPerDay.ToString(), "int", "Librarians",
                    "Maximum number of books a librarian can lend to readers in a single day")
            };
        }
    }
}
