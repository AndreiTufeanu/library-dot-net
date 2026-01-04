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
        /// <summary>Gets a configuration setting by its key.</summary>
        Task<ConfigurationSetting> GetByKeyAsync(string key);

        /// <summary>Gets all configuration settings by category.</summary>
        Task<IEnumerable<ConfigurationSetting>> GetByCategoryAsync(string category);

        /// <summary>Gets the string value of a setting.</summary>
        Task<string> GetValueAsync(string key, string defaultValue = null);

        /// <summary>Gets the integer value of a setting.</summary>
        Task<int> GetIntValueAsync(string key, int defaultValue = 0);

        /// <summary>Gets the double value of a setting.</summary>
        Task<double> GetDoubleValueAsync(string key, double defaultValue = 0.0);

        /// <summary>Gets the timespan value of a setting.</summary>
        Task<TimeSpan> GetTimeSpanValueAsync(string key, TimeSpan defaultValue);

        /// <summary>Gets the boolean value of a setting.</summary>
        Task<bool> GetBoolValueAsync(string key, bool defaultValue = false);
    }
}
