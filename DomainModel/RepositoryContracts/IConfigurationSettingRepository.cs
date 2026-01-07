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
        /// Gets all configuration settings by category
        /// </summary>
        /// <param name="category">The category to filter by</param>
        /// <returns>A collection of configuration settings in the specified category</returns>
        Task<IEnumerable<ConfigurationSetting>> GetByCategoryAsync(string category);

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

        /// <summary>
        /// Gets the double value of a setting
        /// </summary>
        /// <param name="key">The configuration key</param>
        /// <param name="defaultValue">The default value to return if setting is not found or cannot be parsed</param>
        /// <returns>The double value of the setting, or the default value if not found or invalid</returns>
        Task<double> GetDoubleValueAsync(string key, double defaultValue = 0.0);

        /// <summary>
        /// Gets the timespan value of a setting
        /// </summary>
        /// <param name="key">The configuration key</param>
        /// <param name="defaultValue">The default value to return if setting is not found or cannot be parsed</param>
        /// <returns>The TimeSpan value of the setting, or the default value if not found or invalid</returns>
        Task<TimeSpan> GetTimeSpanValueAsync(string key, TimeSpan defaultValue);

        /// <summary>
        /// Gets the boolean value of a setting
        /// </summary>
        /// <param name="key">The configuration key</param>
        /// <param name="defaultValue">The default value to return if setting is not found or cannot be parsed</param>
        /// <returns>The boolean value of the setting, or the default value if not found or invalid</returns>
        Task<bool> GetBoolValueAsync(string key, bool defaultValue = false);
    }
}
