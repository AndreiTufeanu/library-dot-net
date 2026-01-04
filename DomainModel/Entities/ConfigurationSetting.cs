using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainModel.Entities
{
    /// <summary>Represents a configuration setting for the library management system.</summary>
    public class ConfigurationSetting
    {
        /// <summary>Gets or sets the unique identifier.</summary>
        /// <value>A unique GUID that identifies this configuration setting in the system.</value>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Gets or sets the unique key/name of the configuration.</summary>
        /// <value>The configuration key used to retrieve this setting.</value>
        [Required]
        [StringLength(100)]
        [Index(IsUnique = true)]
        public string Key { get; set; }

        /// <summary>Gets or sets the configuration value.</summary>
        /// <value>The actual value of the configuration setting, stored as a string but interpreted according to the DataType.</value>
        [Required]
        public string Value { get; set; }

        /// <summary>Gets or sets the description of the setting.</summary>
        /// <value>An optional description explaining the purpose and usage of this configuration setting.</value>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>Gets or sets the data type of the value.</summary>
        /// <value>The expected data type for interpreting the Value property (e.g., "int", "double", "timespan", "bool").</value>
        [Required]
        [StringLength(50)]
        public string DataType { get; set; }

        /// <summary>Gets or sets the category of the setting.</summary>
        /// <value>The logical grouping category for this setting (e.g., "Books", "Readers", "Librarians").</value>
        [StringLength(100)]
        public string Category { get; set; }

        /// <summary>Gets or sets the last modified date.</summary>
        /// <value>The date and time (in UTC) when this configuration setting was last updated.</value>
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
    }
}