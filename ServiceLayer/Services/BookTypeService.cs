using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using ServiceLayer.Exceptions;
using ServiceLayer.Helpers;
using ServiceLayer.ServiceContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    public class BookTypeService : BaseService, IBookTypeService
    {
        private readonly IBookTypeRepository _repository;
        private readonly IValidator<BookType> _validator;

        public BookTypeService(
            IBookTypeRepository repository,
            IValidator<BookType> validator,
            ILogger<BookTypeService> logger)
            : base(logger)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<ServiceResult<BookType>> CreateAsync(BookType bookType)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                ValidationHelper.Validate(bookType, _validator);

                var existingBookType = await _repository.FindByNameAsync(bookType.Name);
                if (existingBookType != null)
                {
                    throw new Exceptions.ValidationException($"A book type with the name '{bookType.Name}' already exists.");
                }

                return await _repository.AddAsync(bookType);

            }, nameof(CreateAsync));
        }

        public async Task<ServiceResult<BookType>> GetByIdAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var bookType = await _repository.GetByIdAsync(id);
                if (bookType == null)
                {
                    throw new NotFoundException(nameof(BookType), id);
                }
                return bookType;

            }, nameof(GetByIdAsync));
        }

        public async Task<ServiceResult<IEnumerable<BookType>>> GetAllAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _repository.GetAllAsync();

            }, nameof(GetAllAsync));
        }

        public async Task<ServiceResult<bool>> UpdateAsync(BookType bookType)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                if (!await _repository.ExistsAsync(bookType.Id))
                {
                    throw new NotFoundException(nameof(BookType), bookType.Id);
                }

                ValidationHelper.Validate(bookType, _validator);

                var existingBookType = await _repository.FindByNameAsync(bookType.Name);
                if (existingBookType != null && existingBookType.Id != bookType.Id)
                {
                    throw new Exceptions.ValidationException($"Another book type with the name '{bookType.Name}' already exists.");
                }

                await _repository.UpdateAsync(bookType);
                return true;

            }, nameof(UpdateAsync));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var bookType = await _repository.GetByIdAsync(id);
                if (bookType == null)
                {
                    throw new NotFoundException(nameof(BookType), id);
                }

                if (await _repository.HasEditionsAsync(id))
                {
                    throw new BusinessRuleException("Cannot delete a book type that has associated editions.");
                }

                return await _repository.DeleteAsync(id);

            }, nameof(DeleteAsync));
        }

        public async Task<ServiceResult<bool>> ExistsAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _repository.ExistsAsync(id);

            }, nameof(ExistsAsync));
        }
    }
}
