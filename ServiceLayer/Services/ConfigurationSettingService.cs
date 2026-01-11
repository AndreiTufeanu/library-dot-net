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
    public class ConfigurationSettingService : IConfigurationSettingService
    {
        private readonly IConfigurationSettingRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly ConcurrentDictionary<string, object> _defaultValues;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

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
        public async Task<int> GetMaxDomainsPerBookAsync()
        {
            return await GetCachedValueAsync(ConfigurationConstants.MaxDomainsPerBook, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.MaxDomainsPerBook,
                    (int)_defaultValues[ConfigurationConstants.MaxDomainsPerBook]);
            });
        }

        public Task<int> GetDefaultMaxDomainsPerBookAsync()
        {
            return Task.FromResult((int)_defaultValues[ConfigurationConstants.MaxDomainsPerBook]);
        }
        #endregion

        #region Reader Constants
        public async Task<int> GetMaxBooksInPeriodAsync(bool forLibrarian = false)
        {
            var baseValue = await GetCachedValueAsync(ConfigurationConstants.MaxBooksInPeriod, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.MaxBooksInPeriod,
                    (int)_defaultValues[ConfigurationConstants.MaxBooksInPeriod]);
            });

            return forLibrarian ? baseValue * 2 : baseValue;
        }

        public async Task<int> GetMaxBooksInPeriodWindowDaysAsync(bool forLibrarian = false)
        {
            var baseValue = await GetCachedValueAsync(ConfigurationConstants.MaxBooksInPeriodWindowDays, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.MaxBooksInPeriodWindowDays,
                    (int)_defaultValues[ConfigurationConstants.MaxBooksInPeriodWindowDays]);
            });

            return forLibrarian ? baseValue / 2 : baseValue;
        }

        public async Task<int> GetBorrowingPeriodDaysAsync()
        {
            return await GetCachedValueAsync(ConfigurationConstants.BorrowingPeriodDays, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.BorrowingPeriodDays,
                    (int)_defaultValues[ConfigurationConstants.BorrowingPeriodDays]);
            });
        }

        public async Task<int> GetMaxBooksPerBorrowingAsync(bool forLibrarian = false)
        {
            var baseValue = await GetCachedValueAsync(ConfigurationConstants.MaxBooksPerBorrowing, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.MaxBooksPerBorrowing,
                    (int)_defaultValues[ConfigurationConstants.MaxBooksPerBorrowing]);
            });

            return forLibrarian ? baseValue * 2 : baseValue;
        }

        public async Task<int> GetMaxBooksSameDomainAsync(bool forLibrarian = false)
        {
            var baseValue = await GetCachedValueAsync(ConfigurationConstants.MaxBooksSameDomain, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.MaxBooksSameDomain,
                    (int)_defaultValues[ConfigurationConstants.MaxBooksSameDomain]);
            });

            return forLibrarian ? baseValue * 2 : baseValue;
        }

        public async Task<int> GetSameDomainTimeLimitMonthsAsync()
        {
            return await GetCachedValueAsync(ConfigurationConstants.SameDomainTimeLimitMonths, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.SameDomainTimeLimitMonths,
                    (int)_defaultValues[ConfigurationConstants.SameDomainTimeLimitMonths]);
            });
        }

        public async Task<int> GetMaxOvertimeSumDaysAsync(bool forLibrarian = false)
        {
            var baseValue = await GetCachedValueAsync(ConfigurationConstants.MaxOvertimeSumDays, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.MaxOvertimeSumDays,
                    (int)_defaultValues[ConfigurationConstants.MaxOvertimeSumDays]);
            });

            return forLibrarian ? baseValue * 2 : baseValue;
        }

        public async Task<int> GetExtensionWindowMonthsAsync()
        {
            return await GetCachedValueAsync(ConfigurationConstants.ExtensionWindowMonths, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.ExtensionWindowMonths,
                    (int)_defaultValues[ConfigurationConstants.ExtensionWindowMonths]);
            });
        }

        public async Task<int> GetSameBookDelayDaysAsync(bool forLibrarian = false)
        {
            var baseValue = await GetCachedValueAsync(ConfigurationConstants.SameBookDelayDays, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.SameBookDelayDays,
                    (int)_defaultValues[ConfigurationConstants.SameBookDelayDays]);
            });

            return forLibrarian ? baseValue / 2 : baseValue;
        }

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
        public async Task<int> GetMaxBooksLentPerDayAsync()
        {
            return await GetCachedValueAsync(ConfigurationConstants.MaxBooksLentPerDay, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.MaxBooksLentPerDay,
                    (int)_defaultValues[ConfigurationConstants.MaxBooksLentPerDay]);
            });
        }
        #endregion

        public async Task RefreshSettingAsync(string key)
        {
            var cacheKey = $"Config_{key}";
            _cache.Remove(cacheKey);
            await Task.CompletedTask;
        }
    }
}
