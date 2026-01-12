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
    /// <summary>Provides service operations for managing <see cref="Reader"/> entities in the library management system.</summary>
    /// <remarks>
    /// Implements <see cref="IReaderService"/> to handle library patron accounts with comprehensive validation.
    /// Enforces uniqueness of contact information, age requirements, and borrowing eligibility.
    /// </remarks>
    public class ReaderService : BaseService, IReaderService
    {
        /// <summary>The unit of work instance for coordinating repository operations and transactions.</summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>The validator instance for enforcing reader-specific business rules.</summary>
        private readonly IValidator<Reader> _validator;

        /// <summary>Initializes a new instance of the <see cref="ReaderService"/> class.</summary>
        /// <param name="unitOfWork">The unit of work instance for coordinating repository operations.</param>
        /// <param name="validator">The validator instance for enforcing reader-specific business rules.</param>
        /// <param name="logger">The logger instance for logging service operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="unitOfWork"/> or <paramref name="validator"/> is null.</exception>
        public ReaderService(
            IUnitOfWork unitOfWork,
            IValidator<Reader> validator,
            ILogger<ReaderService> logger)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <inheritdoc/>
        /// <exception cref="AggregateValidationException"></exception>
        public async Task<ServiceResult<Reader>> CreateAsync(Reader reader)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                ValidationHelper.Validate(reader, _validator);

                if (!string.IsNullOrWhiteSpace(reader.Email))
                {
                    var existingByEmail = await _unitOfWork.Readers.FindByEmailAsync(reader.Email);
                    if (existingByEmail != null)
                    {
                        throw new AggregateValidationException($"Reader with email '{reader.Email}' already exists.");
                    }
                }

                if (!string.IsNullOrWhiteSpace(reader.PhoneNumber))
                {
                    var existingByPhone = await _unitOfWork.Readers.FindByPhoneAsync(reader.PhoneNumber);
                    if (existingByPhone != null)
                    {
                        throw new AggregateValidationException($"Reader with phone number '{reader.PhoneNumber}' already exists.");
                    }
                }

                var addedReader = await _unitOfWork.Readers.AddAsync(reader);
                await _unitOfWork.SaveChangesAsync();

                return addedReader;

            }, nameof(CreateAsync));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        public async Task<ServiceResult<Reader>> GetByIdAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var reader = await _unitOfWork.Readers.GetByIdAsync(id);
                if (reader == null)
                {
                    throw new NotFoundException(nameof(Reader), id);
                }
                return reader;

            }, nameof(GetByIdAsync));
        }

        /// <inheritdoc/>
        public async Task<ServiceResult<IEnumerable<Reader>>> GetAllAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Readers.GetAllAsync();

            }, nameof(GetAllAsync));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="AggregateValidationException"></exception>
        public async Task<ServiceResult<bool>> UpdateAsync(Reader reader)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                if (!await _unitOfWork.Readers.ExistsAsync(reader.Id))
                {
                    throw new NotFoundException(nameof(Reader), reader.Id);
                }

                ValidationHelper.Validate(reader, _validator);

                if (!string.IsNullOrWhiteSpace(reader.Email))
                {
                    var existingByEmail = await _unitOfWork.Readers.FindByEmailAsync(reader.Email);
                    if (existingByEmail != null && existingByEmail.Id != reader.Id)
                    {
                        throw new AggregateValidationException($"Another reader with email '{reader.Email}' already exists.");
                    }
                }

                if (!string.IsNullOrWhiteSpace(reader.PhoneNumber))
                {
                    var existingByPhone = await _unitOfWork.Readers.FindByPhoneAsync(reader.PhoneNumber);
                    if (existingByPhone != null && existingByPhone.Id != reader.Id)
                    {
                        throw new AggregateValidationException($"Another reader with phone number '{reader.PhoneNumber}' already exists.");
                    }
                }

                await _unitOfWork.Readers.UpdateAsync(reader);
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
                var reader = await _unitOfWork.Readers.GetByIdAsync(id);
                if (reader == null)
                {
                    throw new NotFoundException(nameof(Reader), id);
                }

                if (await _unitOfWork.Readers.HasActiveBorrowingsAsync(id))
                {
                    throw new BusinessRuleException("Cannot delete a reader that has active borrowings.");
                }

                var success = await _unitOfWork.Readers.DeleteAsync(id);
                if (!success)
                {
                    throw new InvalidOperationException("Failed to delete reader");
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
                return await _unitOfWork.Readers.ExistsAsync(id);

            }, nameof(ExistsAsync));
        }
    }
}
