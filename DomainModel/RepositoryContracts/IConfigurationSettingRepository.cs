using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.RepositoryContracts
{
    public interface IConfigurationSettingRepository : IRepository<ConfigurationSetting>
    {
        /// <summary>
        /// Gets a configuration setting by its key
        /// </summary>
        /// <param name="key">The configuration key</param>
        /// <returns>The configuration setting if found; otherwise, null</returns>
        Task<ConfigurationSetting> GetByKeyAsync(string key);

        /// <summary>
        /// Gets the string value of a setting
        /// </summary>
        /// <param name="key">The configuration key</param>
        /// <param name="defaultValue">The default value to return if setting is not found</param>
        /// <returns>The string value of the setting, or the default value if not found</returns>
        Task<string> GetValueAsync(string key, string defaultValue = null);

        /// <summary>
        /// Gets the integer value of a setting
        /// </summary>
        /// <param name="key">The configuration key</param>
        /// <param name="defaultValue">The default value to return if setting is not found or cannot be parsed</param>
        /// <returns>The integer value of the setting, or the default value if not found or invalid</returns>
        Task<int> GetIntValueAsync(string key, int defaultValue = 0);
    }
}
