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
    public class LibrarianService : BaseService, ILibrarianService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LibrarianService(
            IUnitOfWork unitOfWork,
            ILogger<LibrarianService> logger)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

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

        public async Task<ServiceResult<IEnumerable<Librarian>>> GetAllAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Librarians.GetAllAsync();

            }, nameof(GetAllAsync));
        }

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

        public async Task<ServiceResult<bool>> ExistsAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Librarians.ExistsAsync(id);

            }, nameof(ExistsAsync));
        }
    }
}
