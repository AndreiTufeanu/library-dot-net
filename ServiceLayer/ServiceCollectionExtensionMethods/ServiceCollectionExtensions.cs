using DomainModel.Entities;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ServiceLayer.ServiceContracts;
using ServiceLayer.ServiceContracts.HelperServiceContracts;
using ServiceLayer.Services;
using ServiceLayer.Services.HelperServices;
using ServiceLayer.Validators;

namespace ServiceLayer.ServiceCollectionExtensionMethods
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceLayerServices(this IServiceCollection services)
        {
            // Register validators
            services.AddTransient<IValidator<BookType>, BookTypeValidator>();
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
