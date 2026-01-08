using DomainModel.RepositoryContracts;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring Infrastructure Layer dependencies in the dependency injection container.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This static class contains extension methods that register all infrastructure layer components
    /// with the Microsoft Dependency Injection (DI) container. It follows the convention of having
    /// separate extension methods for each architectural layer.
    /// </para>
    /// <para>
    /// This method should be called during application startup in the composition root,
    /// typically from the <c>ConfigureServices</c> method in the startup class, before
    /// registering service layer dependencies.
    /// </para>
    /// <para>
    /// All repositories are registered with scoped lifetime to ensure they share the same
    /// <see cref="ApplicationContext.LibraryContext"/> instance within a single HTTP request or unit of work.
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
    }
}
