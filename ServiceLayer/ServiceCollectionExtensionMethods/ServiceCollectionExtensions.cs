using DomainModel.Entities;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ServiceLayer.ServiceContracts;
using ServiceLayer.Services;
using ServiceLayer.Validators;

namespace ServiceLayer.ServiceCollectionExtensionMethods
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceLayerServices(this IServiceCollection services)
        {
            // Register validators
            services.AddTransient<IValidator<Book>, BookValidator>();
            services.AddTransient<IValidator<BookType>, BookTypeValidator>();
            services.AddTransient<IValidator<Borrowing>, BorrowingValidator>();
            services.AddTransient<IValidator<Domain>, DomainValidator>();
            services.AddTransient<IValidator<Edition>, EditionValidator>();
            services.AddTransient<IValidator<Reader>, ReaderValidator>();

            // Register services
            services.AddScoped<IBookTypeService, BookTypeService>();
            services.AddScoped<IConfigurationSettingService, ConfigurationSettingService>();

            return services;
        }
    }
}
