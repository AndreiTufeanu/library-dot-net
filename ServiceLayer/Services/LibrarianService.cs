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
            IValidator<Librarian> validator,
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

                // If changing ReaderDetails, verify the reader exists and isn't already a librarian
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

                if (librarian.ProcessedLoans != null && librarian.ProcessedLoans.Any())
                {
                    throw new BusinessRuleException("Cannot delete a librarian that has processed loans. Reassign loans first.");
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var success = await _unitOfWork.Librarians.DeleteAsync(id);
                    if (!success)
                    {
                        throw new InvalidOperationException("Failed to delete librarian");
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();
                    return true;
                }
                catch
                {
                    await _unitOfWork.RollbackAsync();
                    throw;
                }

            }, nameof(DeleteAsync));
        }

        public async Task<ServiceResult<bool>> ExistsAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Librarians.ExistsAsync(id);

            }, nameof(ExistsAsync));
        }

        public async Task<ServiceResult<Librarian>> GetByReaderIdAsync(Guid readerId)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var reader = await _unitOfWork.Readers.GetByIdAsync(readerId);
                if (reader == null)
                {
                    throw new NotFoundException(nameof(Reader), readerId);
                }

                var librarian = await _unitOfWork.Librarians.GetByReaderIdAsync(readerId);
                if (librarian == null)
                {
                    throw new NotFoundException($"Librarian for reader ID '{readerId}' not found.");
                }

                return librarian;

            }, nameof(GetByReaderIdAsync));
        }

        public async Task<ServiceResult<bool>> IsReaderAlsoLibrarianAsync(Guid readerId)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var reader = await _unitOfWork.Readers.GetByIdAsync(readerId);
                if (reader == null)
                {
                    throw new NotFoundException(nameof(Reader), readerId);
                }

                return await _unitOfWork.Librarians.IsReaderAlsoLibrarianAsync(readerId);

            }, nameof(IsReaderAlsoLibrarianAsync));
        }

        public async Task<ServiceResult<Librarian>> CreateFromReaderAsync(Guid readerId)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var reader = await _unitOfWork.Readers.GetByIdAsync(readerId);
                if (reader == null)
                {
                    throw new NotFoundException(nameof(Reader), readerId);
                }

                var existingLibrarian = await _unitOfWork.Librarians.GetByReaderIdAsync(readerId);
                if (existingLibrarian != null)
                {
                    throw new AggregateValidationException($"Reader '{reader.FirstName} {reader.LastName}' is already a librarian.");
                }

                var librarian = new Librarian
                {
                    ReaderDetails = reader
                };

                var addedLibrarian = await _unitOfWork.Librarians.AddAsync(librarian);
                await _unitOfWork.SaveChangesAsync();

                return addedLibrarian;

            }, nameof(CreateFromReaderAsync));
        }

        public async Task<ServiceResult<bool>> RemoveLibrarianStatusAsync(Guid librarianId)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var librarian = await _unitOfWork.Librarians.GetByIdAsync(librarianId);
                if (librarian == null)
                {
                    throw new NotFoundException(nameof(Librarian), librarianId);
                }

                if (librarian.ProcessedLoans != null && librarian.ProcessedLoans.Any())
                {
                    throw new BusinessRuleException("Cannot remove librarian status from someone who has processed loans.");
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var success = await _unitOfWork.Librarians.DeleteAsync(librarianId);
                    if (!success)
                    {
                        throw new InvalidOperationException("Failed to remove librarian status");
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();
                    return true;
                }
                catch
                {
                    await _unitOfWork.RollbackAsync();
                    throw;
                }

            }, nameof(RemoveLibrarianStatusAsync));
        }
    }
}
