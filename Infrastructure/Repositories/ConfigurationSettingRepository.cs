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
    public class ConfigurationSettingRepository : IConfigurationSettingRepository
    {
        private readonly LibraryContext _context;

        public ConfigurationSettingRepository(LibraryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ConfigurationSetting> GetByIdAsync(Guid id)
        {
            return await _context.ConfigurationSettings.FindAsync(id);
        }

        public async Task<IEnumerable<ConfigurationSetting>> GetAllAsync()
        {
            return await _context.ConfigurationSettings.ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.ConfigurationSettings.AnyAsync(cs => cs.Id == id);
        }

        public async Task<ConfigurationSetting> AddAsync(ConfigurationSetting entity)
        {
            var addedEntity = _context.ConfigurationSettings.Add(entity);
            await _context.SaveChangesAsync();
            return addedEntity;
        }

        public async Task<ConfigurationSetting> UpdateAsync(ConfigurationSetting entity)
        {
            entity.LastModified = DateTime.UtcNow;
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var setting = await _context.ConfigurationSettings.FindAsync(id);
            if (setting == null) return false;

            _context.ConfigurationSettings.Remove(setting);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ConfigurationSetting> GetByKeyAsync(string key)
        {
            return await _context.ConfigurationSettings
                .FirstOrDefaultAsync(cs => cs.Key == key);
        }

        public async Task<string> GetValueAsync(string key, string defaultValue = null)
        {
            var setting = await GetByKeyAsync(key);
            return setting?.Value ?? defaultValue;
        }

        public async Task<int> GetIntValueAsync(string key, int defaultValue = 0)
        {
            var value = await GetValueAsync(key);
            return int.TryParse(value, out int result) ? result : defaultValue;
        }
    }
}
