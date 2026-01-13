using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using Infrastructure.ApplicationContext;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    /// <summary>Provides data access operations for <see cref="ConfigurationSetting"/> entities.</summary>
    /// <remarks>
    /// Implements <see cref="IConfigurationSettingRepository"/> to provide CRUD operations and configuration-specific queries.
    /// Manages system configuration settings with automatic timestamp updates on modifications.
    /// </remarks>

    public class ConfigurationSettingRepository : IConfigurationSettingRepository
    {
        /// <summary>The Entity Framework database context for accessing configuration setting data.</summary>
        private readonly LibraryContext _context;

        /// <summary>Initializes a new instance of the <see cref="ConfigurationSettingRepository"/> class.</summary>
        /// <param name="context">The database context for accessing configuration setting data.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        public ConfigurationSettingRepository(LibraryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<ConfigurationSetting> GetByIdAsync(Guid id)
        {
            return await _context.ConfigurationSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(cs => cs.Id == id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ConfigurationSetting>> GetAllAsync()
        {
            return await _context.ConfigurationSettings.AsNoTracking().ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.ConfigurationSettings.AnyAsync(cs => cs.Id == id);
        }

        /// <inheritdoc/>
        public async Task<ConfigurationSetting> AddAsync(ConfigurationSetting entity)
        {
            var addedEntity = _context.ConfigurationSettings.Add(entity);
            await _context.SaveChangesAsync();
            return addedEntity;
        }

        /// <inheritdoc/>
        public async Task<ConfigurationSetting> UpdateAsync(ConfigurationSetting entity)
        {
            entity.LastModified = DateTime.UtcNow;
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var setting = await _context.ConfigurationSettings.FindAsync(id);
            if (setting == null) return false;

            _context.ConfigurationSettings.Remove(setting);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<ConfigurationSetting> GetByKeyAsync(string key)
        {
            return await _context.ConfigurationSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(cs => cs.Key == key);
        }

        /// <inheritdoc/>
        public async Task<string> GetValueAsync(string key, string defaultValue = null)
        {
            var setting = await GetByKeyAsync(key);
            return setting?.Value ?? defaultValue;
        }

        /// <inheritdoc/>
        public async Task<int> GetIntValueAsync(string key, int defaultValue = 0)
        {
            var value = await GetValueAsync(key);
            return int.TryParse(value, out int result) ? result : defaultValue;
        }
    }
}
