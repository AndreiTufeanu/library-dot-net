using DomainModel.RepositoryContracts;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ServiceCollectionExtensionMethods
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
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
