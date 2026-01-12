using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using ServiceLayer.Exceptions;
using ServiceLayer.Helpers;
using ServiceLayer.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    /// <summary>Provides service operations for managing <see cref="Librarian"/> entities in the library management system.</summary>
    /// <remarks>
    /// Implements <see cref="ILibrarianService"/> to handle librarian accounts and their associated reader profiles.
    /// Ensures each reader can be associated with at most one librarian account.
    /// </remarks>
    public class LibrarianService : BaseService, ILibrarianService
    {
        /// <summary>The unit of work instance for coordinating repository operations and transactions.</summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>Initializes a new instance of the <see cref="LibrarianService"/> class.</summary>
        /// <param name="unitOfWork">The unit of work instance for coordinating repository operations.</param>
        /// <param name="logger">The logger instance for logging service operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="unitOfWork"/> is null.</exception>
        public LibrarianService(
            IUnitOfWork unitOfWork,
            ILogger<LibrarianService> logger)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="AggregateValidationException"></exception>
        public async Task<ServiceResult<Librarian>> CreateAsync(Librarian librarian)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                ValidationHelper.Validate(librarian);

                if (librarian.ReaderDetails != null)
                {
                    var reader = await _unitOfWork.Readers.GetByIdAsync(librarian.ReaderDetails.Id);
                    if (reader == null)
                    {
                        throw new NotFoundException(nameof(Reader), librarian.ReaderDetails.Id);
                    }

                    var existingLibrarian = await _unitOfWork.Librarians.GetByReaderIdAsync(reader.Id);
                    if (existingLibrarian != null)
                    {
                        throw new AggregateValidationException($"Reader '{reader.FirstName} {reader.LastName}' is already a librarian.");
                    }
                }

                var addedLibrarian = await _unitOfWork.Librarians.AddAsync(librarian);
                await _unitOfWork.SaveChangesAsync();

                return addedLibrarian;

            }, nameof(CreateAsync));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        public async Task<ServiceResult<Librarian>> GetByIdAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var librarian = await _unitOfWork.Librarians.GetByIdAsync(id);
                if (librarian == null)
                {
                    throw new NotFoundException(nameof(Librarian), id);
                }
                return librarian;

            }, nameof(GetByIdAsync));
        }

        /// <inheritdoc/>
        public async Task<ServiceResult<IEnumerable<Librarian>>> GetAllAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Librarians.GetAllAsync();

            }, nameof(GetAllAsync));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="AggregateValidationException"></exception>
        public async Task<ServiceResult<bool>> UpdateAsync(Librarian librarian)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                if (!await _unitOfWork.Librarians.ExistsAsync(librarian.Id))
                {
                    throw new NotFoundException(nameof(Librarian), librarian.Id);
                }

                ValidationHelper.Validate(librarian);

                if (librarian.ReaderDetails != null)
                {
                    var reader = await _unitOfWork.Readers.GetByIdAsync(librarian.ReaderDetails.Id);
                    if (reader == null)
                    {
                        throw new NotFoundException(nameof(Reader), librarian.ReaderDetails.Id);
                    }

                    var existingLibrarian = await _unitOfWork.Librarians.GetByReaderIdAsync(reader.Id);
                    if (existingLibrarian != null && existingLibrarian.Id != librarian.Id)
                    {
                        throw new AggregateValidationException($"Reader '{reader.FirstName} {reader.LastName}' is already assigned to another librarian.");
                    }
                }

                await _unitOfWork.Librarians.UpdateAsync(librarian);
                await _unitOfWork.SaveChangesAsync();
                return true;

            }, nameof(UpdateAsync));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="BusinessRuleException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<ServiceResult<bool>> DeleteAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var librarian = await _unitOfWork.Librarians.GetByIdAsync(id);
                if (librarian == null)
                {
                    throw new NotFoundException(nameof(Librarian), id);
                }

                if (librarian.ProcessedLoans.Any())
                {
                    throw new BusinessRuleException("Cannot delete a librarian that has processed loans. Reassign loans first.");
                }

                var success = await _unitOfWork.Librarians.DeleteAsync(id);
                if (!success)
                {
                    throw new InvalidOperationException("Failed to delete librarian");
                }

                await _unitOfWork.SaveChangesAsync();
                return true;

            }, nameof(DeleteAsync));
        }

        /// <inheritdoc/>
        public async Task<ServiceResult<bool>> ExistsAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Librarians.ExistsAsync(id);

            }, nameof(ExistsAsync));
        }
    }
}
