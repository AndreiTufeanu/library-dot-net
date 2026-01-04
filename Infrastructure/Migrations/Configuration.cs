namespace Infrastructure.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using DomainModel.Entities;
    using ServiceLayer.Helpers;

    internal sealed class Configuration : DbMigrationsConfiguration<Infrastructure.ApplicationContext.LibraryContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Infrastructure.ApplicationContext.LibraryContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
            // Use ConfigurationConstants to get all default settings
            var defaultSettings = new List<ConfigurationSetting>();

            foreach (var (key, value, dataType, category, description) in ConfigurationConstants.GetAllDefaults())
            {
                defaultSettings.Add(new ConfigurationSetting
                {
                    Key = key,
                    Value = value,
                    DataType = dataType,
                    Category = category,
                    Description = description,
                    LastModified = DateTime.UtcNow
                });
            }

            foreach (var setting in defaultSettings)
            {
                context.ConfigurationSettings.AddOrUpdate(s => s.Key, setting);
            }
        }
    }
}
