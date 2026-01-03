using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ServiceLayer.Validators;
using System;
using DomainModel.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceExtensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceLayerServices(this IServiceCollection services)
        {
            services.AddTransient<IValidator<BookType>, BookTypeValidator>();

            services.AddScoped<ServiceContracts.IBookTypeService, Services.BookTypeService>();

            return services;
        }
    }
}
