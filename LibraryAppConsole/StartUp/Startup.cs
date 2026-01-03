using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using ServiceLayer.ServiceExtensions;
using Infrastructure.ServiceExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.ApplicationContext;

namespace LibraryAppConsole.StartUp
{
    public class Startup
    {
        public static IServiceProvider ConfigureServices()
        {
            // Configure Serilog to log to console
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

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
    }
}
