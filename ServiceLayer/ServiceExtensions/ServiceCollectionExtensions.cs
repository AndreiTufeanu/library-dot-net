using DomainModel.Entities;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ServiceLayer.ServiceContracts;
using ServiceLayer.Services;
using ServiceLayer.Validators;

namespace ServiceLayer.ServiceExtensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceLayerServices(this IServiceCollection services)
        {
            // Register validators
            services.AddTransient<IValidator<BookType>, BookTypeValidator>();

            // Register services
            services.AddScoped<IBookTypeService, BookTypeService>();
            services.AddScoped<IConfigurationSettingService, ConfigurationSettingService>();

            return services;
        }
    }
}
