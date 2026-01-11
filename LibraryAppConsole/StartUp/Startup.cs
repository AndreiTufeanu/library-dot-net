using Infrastructure.ApplicationContext;
using Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryAppConsole.StartUp
{
    /// <summary>
    /// Provides application startup configuration and dependency injection setup for the Library Management Console Application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is responsible for configuring the application's dependency injection container,
    /// setting up logging infrastructure using Serilog, and establishing the database connection.
    /// It serves as the composition root for the application.
    /// </para>
    /// </remarks>
    public class Startup
    {
        /// <summary>
        /// Configures and builds the dependency injection container for the application.
        /// </summary>
        /// <returns>
        /// An <see cref="IServiceProvider"/> configured with all application services.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the "LibraryDBConnectionString" is not configured in the application configuration file.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method performs the following operations:
        /// <list type="number">
        /// <item><description>Configures Serilog logging with console and SQL Server sinks</description></item>
        /// <item><description>Creates a new <see cref="IServiceCollection"/> instance</description></item>
        /// <item><description>Adds logging services to the DI container</description></item>
        /// <item><description>Registers infrastructure layer services (repositories, DbContext)</description></item>
        /// <item><description>Registers service layer services (business services, validators)</description></item>
        /// <item><description>Builds and returns the service provider</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// The method reads the database connection string from the application configuration file
        /// (app.config or web.config) using the key "LibraryDBConnectionString".
        /// </para>
        /// </remarks>
        public static IServiceProvider ConfigureServices()
        {
            // Configure Serilog
            ConfigureLogging();

            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddSerilog(dispose: true);
            });

            // Add Infrastructure services
            services.AddInfrastructureServices();

            // Add ServiceLayer services
            services.AddServiceLayerServices();

            // Add DbContext
            services.AddScoped<LibraryContext>();

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Configures the Serilog logging infrastructure for the application.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the "LibraryDBConnectionString" is not configured in the application configuration file.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method configures Serilog with two sinks:
        /// <list type="bullet">
        /// <item><description><b>Console sink</b>: Logs to the console with a formatted output template for development and debugging</description></item>
        /// <item><description><b>MSSQL Server sink</b>: Logs to a SQL Server database table named "ApplicationLogs" for production monitoring</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// The following log levels are configured:
        /// <list type="bullet">
        /// <item><description><b>Console</b>: Information level and above</description></item>
        /// <item><description><b>Database</b>: Warning level and above (to avoid flooding the database with informational logs)</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        private static void ConfigureLogging()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LibraryDBConnectionString"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("LibraryDBConnectionString is not configured in app.config");
            }

            var sqlSinkOptions = new MSSqlServerSinkOptions
            {
                TableName = "ApplicationLogs",
                AutoCreateSqlTable = true,
                BatchPostingLimit = 50,
                BatchPeriod = TimeSpan.FromSeconds(2)
            };

            var columnOptions = new ColumnOptions
            {
                AdditionalColumns = new Collection<SqlColumn>
                {
                    new SqlColumn { ColumnName = "MachineName", DataType = SqlDbType.NVarChar, DataLength = 50 },
                    new SqlColumn { ColumnName = "SourceContext", DataType = SqlDbType.NVarChar, DataLength = 128 }
                }
            };

            columnOptions.Store.Remove(StandardColumn.Properties);
            columnOptions.Store.Remove(StandardColumn.Id);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.MSSqlServer(
                    connectionString: connectionString,
                    sinkOptions: sqlSinkOptions,
                    restrictedToMinimumLevel: LogEventLevel.Warning,
                    columnOptions: columnOptions)
                .CreateLogger();
        }
    }
}
