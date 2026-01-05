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
            _repository = repository;
            _cache = cache;

            // Initialize default values using ConfigurationConstants
            _defaultValues = new ConcurrentDictionary<string, object>
            {
                [ConfigurationConstants.MaxDomainsPerBook] = ConfigurationConstants.DefaultMaxDomainsPerBook,
                [ConfigurationConstants.MaxBooksInPeriod] = ConfigurationConstants.DefaultMaxBooksInPeriod,
                [ConfigurationConstants.BorrowingPeriodDays] = ConfigurationConstants.DefaultBorrowingPeriodDays,
                [ConfigurationConstants.MaxBooksPerBorrowing] = ConfigurationConstants.DefaultMaxBooksPerBorrowing,
                [ConfigurationConstants.MaxBooksSameDomain] = ConfigurationConstants.DefaultMaxBooksSameDomain,
                [ConfigurationConstants.SameDomainTimeLimitDays] = ConfigurationConstants.DefaultSameDomainTimeLimitDays,
                [ConfigurationConstants.MaxOvertimeSumDays] = ConfigurationConstants.DefaultMaxOvertimeSumDays,
                [ConfigurationConstants.SameBookDelayDays] = ConfigurationConstants.DefaultSameBookDelayDays,
                [ConfigurationConstants.MaxBooksPerDay] = ConfigurationConstants.DefaultMaxBooksPerDay,
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
        public async Task<int> GetMaxBooksInPeriodAsync()
        {
            return await GetCachedValueAsync(ConfigurationConstants.MaxBooksInPeriod, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.MaxBooksInPeriod,
                    (int)_defaultValues[ConfigurationConstants.MaxBooksInPeriod]);
            });
        }

        public async Task<TimeSpan> GetBorrowingPeriodAsync()
        {
            return await GetCachedValueAsync(ConfigurationConstants.BorrowingPeriodDays, async () =>
            {
                var days = await _repository.GetDoubleValueAsync(ConfigurationConstants.BorrowingPeriodDays,
                    (double)_defaultValues[ConfigurationConstants.BorrowingPeriodDays]);
                return TimeSpan.FromDays(days);
            });
        }

        public async Task<int> GetMaxBooksPerBorrowingAsync()
        {
            return await GetCachedValueAsync(ConfigurationConstants.MaxBooksPerBorrowing, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.MaxBooksPerBorrowing,
                    (int)_defaultValues[ConfigurationConstants.MaxBooksPerBorrowing]);
            });
        }

        public async Task<int> GetMaxBooksSameDomainAsync()
        {
            return await GetCachedValueAsync(ConfigurationConstants.MaxBooksSameDomain, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.MaxBooksSameDomain,
                    (int)_defaultValues[ConfigurationConstants.MaxBooksSameDomain]);
            });
        }

        public async Task<TimeSpan> GetSameDomainTimeLimitAsync()
        {
            return await GetCachedValueAsync(ConfigurationConstants.SameDomainTimeLimitDays, async () =>
            {
                var days = await _repository.GetDoubleValueAsync(ConfigurationConstants.SameDomainTimeLimitDays,
                    (double)_defaultValues[ConfigurationConstants.SameDomainTimeLimitDays]);
                return TimeSpan.FromDays(days);
            });
        }

        public async Task<int> GetMaxOvertimeSumAsync()
        {
            return await GetCachedValueAsync(ConfigurationConstants.MaxOvertimeSumDays, async () =>
            {
                return await _repository.GetIntValueAsync(ConfigurationConstants.MaxOvertimeSumDays,
                    (int)_defaultValues[ConfigurationConstants.MaxOvertimeSumDays]);
            });
        }

        public async Task<TimeSpan> GetSameBookDelayAsync()
        {
            return await GetCachedValueAsync(ConfigurationConstants.SameBookDelayDays, async () =>
            {
                var days = await _repository.GetDoubleValueAsync(ConfigurationConstants.SameBookDelayDays,
                    (double)_defaultValues[ConfigurationConstants.SameBookDelayDays]);
                return TimeSpan.FromDays(days);
            });
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
