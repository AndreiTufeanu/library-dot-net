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
    public class ReaderService : BaseService, IReaderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<Reader> _validator;

        public ReaderService(
            IUnitOfWork unitOfWork,
            IValidator<Reader> validator,
            ILogger<ReaderService> logger)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

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

        public async Task<ServiceResult<IEnumerable<Reader>>> GetAllAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Readers.GetAllAsync();

            }, nameof(GetAllAsync));
        }

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

        public async Task<ServiceResult<bool>> ExistsAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Readers.ExistsAsync(id);

            }, nameof(ExistsAsync));
        }
    }
}
