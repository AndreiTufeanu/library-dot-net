using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using FluentValidation;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using ServiceLayer.ServiceContracts;
using ServiceLayer.ServiceContracts.HelperServiceContracts;
using ServiceLayer.Services;
using ServiceLayer.Services.HelperServices;
using Infrastructure.ApplicationContext;
using ServiceLayer.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring Infrastructure and Service Layer dependencies
    /// in the Microsoft dependency injection container (<see cref="IServiceCollection"/>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This static class exposes two extension methods:
    /// <list type="bullet">
    ///   <item><see cref="AddInfrastructureServices(IServiceCollection)"/> — registers repositories, unit of work and any infrastructure-specific services (scoped lifetime).</item>
    ///   <item><see cref="AddServiceLayerServices(IServiceCollection)"/> — registers validators, helper services and the main service layer types (mix of transient and scoped lifetimes).</item>
    /// </list>
    /// </para>
    /// <para>
    /// This method should be called during application startup.
    /// </para>
    /// <para>
    /// All repositories are registered with scoped lifetime to ensure they share the same
    /// <see cref="LibraryContext"/> instance within a single HTTP request or unit of work.
    /// </para>
    /// </remarks>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all Infrastructure Layer services with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IAuthorRepository, AuthorRepository>();
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IBookCopyRepository, BookCopyRepository>();
            services.AddScoped<IBookTypeRepository, BookTypeRepository>();
            services.AddScoped<IBorrowingRepository, BorrowingRepository>();
            services.AddScoped<IConfigurationSettingRepository, ConfigurationSettingRepository>();
            services.AddScoped<IDomainRepository, DomainRepository>();
            services.AddScoped<IEditionRepository, EditionRepository>();
            services.AddScoped<ILibrarianRepository, LibrarianRepository>();
            services.AddScoped<IReaderRepository, ReaderRepository>();

            services.AddMemoryCache();
            return services;
        }

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
