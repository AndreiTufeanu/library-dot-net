using Infrastructure.ApplicationContext;
using Infrastructure.ServiceCollectionExtensionMethods;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using ServiceLayer.ServiceCollectionExtensionMethods;
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
    public class Startup
    {
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
