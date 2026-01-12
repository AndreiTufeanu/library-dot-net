using DomainModel.RepositoryContracts;
using Microsoft.Extensions.Caching.Memory;
using ServiceLayer.Helpers;
using ServiceLayer.ServiceContracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    /// <summary>Provides centralized configuration management for the library management system.</summary>
    /// <remarks>
    /// Implements <see cref="IConfigurationSettingService"/> to retrieve and cache business rule parameters.
    /// Provides intelligent caching and applies privilege adjustments for librarian users.
    /// </remarks>
    public class ConfigurationSettingService : IConfigurationSettingService
    {
        /// <summary>The repository for accessing configuration settings from the database.</summary>
        private readonly IConfigurationSettingRepository _repository;

        /// <summary>The memory cache instance for caching configuration values.</summary>
        private readonly IMemoryCache _cache;

        /// <summary>The dictionary containing default configuration values from <see cref="ConfigurationConstants"/>.</summary>
        private readonly ConcurrentDictionary<string, object> _defaultValues;

        /// <summary>The cache duration for configuration values.</summary>
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

        /// <summary>Initializes a new instance of the <see cref="ConfigurationSettingService"/> class.</summary>
        /// <param name="repository">The repository for accessing configuration settings from the database.</param>
        /// <param name="cache">The memory cache instance for caching configuration values.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> or <paramref name="cache"/> is null.</exception>
        /// <remarks>Initializes default values from <see cref="ConfigurationConstants"/> for fallback when database configuration is unavailable.</remarks>
        public ConfigurationSettingService(IConfigurationSettingRepository repository, IMemoryCache cache)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

            // Initialize default values using ConfigurationConstants
            _defaultValues = new ConcurrentDictionary<string, object>
            {
                // Book Constants
                [ConfigurationConstants.MaxDomainsPerBook] = ConfigurationConstants.DefaultMaxDomainsPerBook,

                // Reader Constants
                [ConfigurationConstants.MaxBooksInPeriod] = ConfigurationConstants.DefaultMaxBooksInPeriod,
                [ConfigurationConstants.MaxBooksInPeriodWindowDays] = ConfigurationConstants.DefaultMaxBooksInPeriodWindowDays,
                [ConfigurationConstants.BorrowingPeriodDays] = ConfigurationConstants.DefaultBorrowingPeriodDays,
                [ConfigurationConstants.MaxBooksPerBorrowing] = ConfigurationConstants.DefaultMaxBooksPerBorrowing,
                [ConfigurationConstants.MaxBooksSameDomain] = ConfigurationConstants.DefaultMaxBooksSameDomain,
                [ConfigurationConstants.SameDomainTimeLimitMonths] = ConfigurationConstants.DefaultSameDomainTimeLimitMonths,
                [ConfigurationConstants.MaxOvertimeSumDays] = ConfigurationConstants.DefaultMaxOvertimeSumDays,
                [ConfigurationConstants.ExtensionWindowMonths] = ConfigurationConstants.DefaultExtensionWindowMonths,
                [ConfigurationConstants.SameBookDelayDays] = ConfigurationConstants.DefaultSameBookDelayDays,
                [ConfigurationConstants.MaxBooksPerDay] = ConfigurationConstants.DefaultMaxBooksPerDay,

                // Librarian Constants
                [ConfigurationConstants.MaxBooksLentPerDay] = ConfigurationConstants.DefaultMaxBooksLentPerDay
            };
        }

        /// <summary>Retrieves a configuration value from cache or repository, implementing the cache-aside pattern.</summary>
        /// <typeparam name="T">The type of the configuration value to retrieve.</typeparam>
        /// <param name="key">The configuration key to look up.</param>
        /// <param name="getter">The asynchronous function to retrieve the value from the repository if not cached.</param>
        /// <returns>The configuration value of type <typeparamref name="T"/>.</returns>
        /// <remarks>
        /// <para>
        /// This method implements the cache-aside pattern: it first checks the memory cache for the value.
        /// If found, it returns the cached value; otherwise, it calls the provided <paramref name="getter"/>
        /// function to retrieve the value from the repository, caches it with a 30-minute expiration,
        /// and then returns it.
        /// </para>
        /// <para>
        /// Cache keys are formatted as "Config_{key}" to avoid collisions with other cached items.
        /// The method is generic to support different configuration value types (int, string, etc.).
        /// </para>
        /// <para>
        /// If the <paramref name="getter"/> function throws an exception, the exception is propagated
        /// and no value is cached, ensuring stale data is not stored in the cache.
        /// </para>
        /// </remarks>
        private async Task<T> GetCachedValueAsync<T>(string key, Func<Task<T>> getter)
        {
            var cacheKey = $"Config_{key}";

            if (!_cache.TryGetValue(cacheKey, out T value))
            {
                value = await getter();
                _cache.Set(cacheKey, value, _cacheDuration);
            }

            return value;
        }

        #region Book Constants

        /// <inheritdoc/>
        public async Task<int> GetMaxDomainsPerBookAsync()
        {
            return await GetCachedValueAsync(ConfigurationConstants.MaxDomainsPerBook, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.MaxDomainsPerBook,
                    (int)_defaultValues[ConfigurationConstants.MaxDomainsPerBook]);
            });
        }

        #endregion

        #region Reader Constants

        /// <inheritdoc/>
        public async Task<int> GetMaxBooksInPeriodAsync(bool forLibrarian = false)
        {
            var baseValue = await GetCachedValueAsync(ConfigurationConstants.MaxBooksInPeriod, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.MaxBooksInPeriod,
                    (int)_defaultValues[ConfigurationConstants.MaxBooksInPeriod]);
            });

            return forLibrarian ? baseValue * 2 : baseValue;
        }

        /// <inheritdoc/>
        public async Task<int> GetMaxBooksInPeriodWindowDaysAsync(bool forLibrarian = false)
        {
            var baseValue = await GetCachedValueAsync(ConfigurationConstants.MaxBooksInPeriodWindowDays, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.MaxBooksInPeriodWindowDays,
                    (int)_defaultValues[ConfigurationConstants.MaxBooksInPeriodWindowDays]);
            });

            return forLibrarian ? baseValue / 2 : baseValue;
        }

        /// <inheritdoc/>
        public async Task<int> GetBorrowingPeriodDaysAsync()
        {
            return await GetCachedValueAsync(ConfigurationConstants.BorrowingPeriodDays, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.BorrowingPeriodDays,
                    (int)_defaultValues[ConfigurationConstants.BorrowingPeriodDays]);
            });
        }

        /// <inheritdoc/>
        public async Task<int> GetMaxBooksPerBorrowingAsync(bool forLibrarian = false)
        {
            var baseValue = await GetCachedValueAsync(ConfigurationConstants.MaxBooksPerBorrowing, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.MaxBooksPerBorrowing,
                    (int)_defaultValues[ConfigurationConstants.MaxBooksPerBorrowing]);
            });

            return forLibrarian ? baseValue * 2 : baseValue;
        }

        /// <inheritdoc/>
        public async Task<int> GetMaxBooksSameDomainAsync(bool forLibrarian = false)
        {
            var baseValue = await GetCachedValueAsync(ConfigurationConstants.MaxBooksSameDomain, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.MaxBooksSameDomain,
                    (int)_defaultValues[ConfigurationConstants.MaxBooksSameDomain]);
            });

            return forLibrarian ? baseValue * 2 : baseValue;
        }

        /// <inheritdoc/>
        public async Task<int> GetSameDomainTimeLimitMonthsAsync()
        {
            return await GetCachedValueAsync(ConfigurationConstants.SameDomainTimeLimitMonths, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.SameDomainTimeLimitMonths,
                    (int)_defaultValues[ConfigurationConstants.SameDomainTimeLimitMonths]);
            });
        }

        /// <inheritdoc/>
        public async Task<int> GetMaxOvertimeSumDaysAsync(bool forLibrarian = false)
        {
            var baseValue = await GetCachedValueAsync(ConfigurationConstants.MaxOvertimeSumDays, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.MaxOvertimeSumDays,
                    (int)_defaultValues[ConfigurationConstants.MaxOvertimeSumDays]);
            });

            return forLibrarian ? baseValue * 2 : baseValue;
        }

        /// <inheritdoc/>
        public async Task<int> GetExtensionWindowMonthsAsync()
        {
            return await GetCachedValueAsync(ConfigurationConstants.ExtensionWindowMonths, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.ExtensionWindowMonths,
                    (int)_defaultValues[ConfigurationConstants.ExtensionWindowMonths]);
            });
        }

        /// <inheritdoc/>
        public async Task<int> GetSameBookDelayDaysAsync(bool forLibrarian = false)
        {
            var baseValue = await GetCachedValueAsync(ConfigurationConstants.SameBookDelayDays, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.SameBookDelayDays,
                    (int)_defaultValues[ConfigurationConstants.SameBookDelayDays]);
            });

            return forLibrarian ? baseValue / 2 : baseValue;
        }

        /// <inheritdoc/>
        public async Task<int> GetMaxBooksPerDayAsync()
        {
            return await GetCachedValueAsync(ConfigurationConstants.MaxBooksPerDay, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.MaxBooksPerDay,
                    (int)_defaultValues[ConfigurationConstants.MaxBooksPerDay]);
            });
        }
        #endregion

        #region Librarian Constants

        /// <inheritdoc/>
        public async Task<int> GetMaxBooksLentPerDayAsync()
        {
            return await GetCachedValueAsync(ConfigurationConstants.MaxBooksLentPerDay, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.MaxBooksLentPerDay,
                    (int)_defaultValues[ConfigurationConstants.MaxBooksLentPerDay]);
            });
        }
        #endregion

        /// <inheritdoc/>
        public async Task RefreshSettingAsync(string key)
        {
            var cacheKey = $"Config_{key}";
            _cache.Remove(cacheKey);
            await Task.CompletedTask;
        }
    }
}
