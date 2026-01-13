using System;
using ServiceLayer.ServiceContracts;
using ServiceLayer.Services;
using System.Collections.Generic;
using System.Linq;
using DomainModel.Entities;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using LibraryAppConsole.StartUp;

namespace LibraryAppConsole
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = Startup.ConfigureServices();

            using (var scope = serviceProvider.CreateScope())
            {
                var bookService = scope.ServiceProvider.GetRequiredService<IBookService>();
                var domainService = scope.ServiceProvider.GetRequiredService<IDomainService>();

                try
                {
                    Console.WriteLine("=== Book CRUD Operations Demo ===\n");

                    //// 1. FAILED CREATE: No domain assigned
                    Console.WriteLine("1. Failed Create (Validation Error):");
                    var invalidBook = new Book(initialCopies: 5)
                    {
                        Title = "Invalid Book",
                        Description = "This book has no domain assigned"
                    };

                    var failedResult = await bookService.CreateAsync(invalidBook);
                    Console.WriteLine($"{failedResult.ErrorMessage}\n");

                    // 2. SUCCESSFUL CREATE
                    Console.WriteLine("2. Successful Create:");

                    await domainService.CreateAsync(new Domain
                    {
                        Name = "Fiction"
                    });

                    // Get an existing domain
                    var domains = (await domainService.GetAllAsync()).Data;
                    var domain = domains.FirstOrDefault();

                    if (domain == null)
                    {
                        Console.WriteLine("No domains exist in database!");
                        return;
                    }

                    var newBook = new Book(initialCopies: 10)
                    {
                        Title = "Test Book Title",
                        Description = "A novel",
                    };
                    newBook.Domains.Add(domain);

                    var createResult = await bookService.CreateAsync(newBook);
                    if (createResult.Success)
                    {
                        var createdBook = createResult.Data;
                        Console.WriteLine($"Created: {createdBook.Title} (ID: {createdBook.Id})\n");

                        // 3. SUCCESSFUL UPDATE
                        Console.WriteLine("3. Successful Update:");
                        createdBook.Title = "Updated title - Revised Edition";
                        var updateResult = await bookService.UpdateAsync(createdBook);
                        Console.WriteLine($"Updated title to: {createdBook.Title}\n");

                        // 4. SUCCESSFUL DELETE
                        Console.WriteLine("4. Successful Delete:");
                        var deleteResult = await bookService.DeleteAsync(createdBook.Id);
                        Console.WriteLine($"Book deleted successfully\n");

                        // 5. FAILED GET BY ID (book is deleted)
                        Console.WriteLine("5. Failed Get By ID (Book Deleted):");
                        var getResult = await bookService.GetByIdAsync(createdBook.Id);
                        Console.WriteLine($"{getResult.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                }

                Console.WriteLine("\nDemo completed. Press any key to exit.");
                Console.ReadKey();
            }
        }
    }
}
