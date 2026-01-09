using DomainModel.Entities;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ServiceLayer.ServiceContracts;
using ServiceLayer.ServiceContracts.HelperServiceContracts;
using ServiceLayer.Services;
using ServiceLayer.Services.HelperServices;
using ServiceLayer.Validators;

namespace ServiceLayer.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring Service Layer dependencies in the dependency injection container.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This static class contains extension methods that register all service layer components
    /// with the Microsoft Dependency Injection (DI) container. It follows the convention of
    /// having separate extension methods for each architectural layer.
    /// </para>
    /// <para>
    /// This method should be called during application startup in the composition root,
    /// typically from the <c>ConfigureServices</c> method in the startup class.
    /// </para>
    /// <para>
    /// </para>
    /// </remarks>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all Service Layer services with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddServiceLayerServices(this IServiceCollection services)
        {
            // Register validators
            services.AddTransient<IValidator<Book>, BookValidator>();
            services.AddTransient<IValidator<Borrowing>, BorrowingValidator>();
            services.AddTransient<IValidator<Domain>, DomainValidator>();
            services.AddTransient<IValidator<Edition>, EditionValidator>();
            services.AddTransient<IValidator<Reader>, ReaderValidator>();

            // Register helper services
            services.AddScoped<IBookHelperService, BookHelperService>();
            services.AddScoped<IBorrowingHelperService, BorrowingHelperService>();

            // Register main services
            services.AddScoped<IAuthorService, AuthorService>();
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IBookCopyService, BookCopyService>();
            services.AddScoped<IBookTypeService, BookTypeService>();
            services.AddScoped<IConfigurationSettingService, ConfigurationSettingService>();
            services.AddScoped<IDomainService, DomainService>();
            services.AddScoped<IEditionService, EditionService>();
            services.AddScoped<IReaderService, ReaderService>();
            services.AddScoped<ILibrarianService, LibrarianService>();

            return services;
        }
    }
}
