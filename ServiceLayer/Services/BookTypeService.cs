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
        private readonly IUnitOfWork _unitOfWork;

        public BookTypeService(
            IUnitOfWork unitOfWork,
            ILogger<BookTypeService> logger)
            : base(logger)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResult<BookType>> CreateAsync(BookType bookType)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                ValidationHelper.Validate(bookType);

                var existingBookType = await _unitOfWork.BookTypes.FindByNameAsync(bookType.Name);
                if (existingBookType != null)
                {
                    throw new AggregateValidationException($"A book type with the name '{bookType.Name}' already exists.");
                }

                var addedBookType = await _unitOfWork.BookTypes.AddAsync(bookType);
                await _unitOfWork.SaveChangesAsync();

                return addedBookType;

            }, nameof(CreateAsync));
        }

        public async Task<ServiceResult<BookType>> GetByIdAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var bookType = await _unitOfWork.BookTypes.GetByIdAsync(id);
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
                return await _unitOfWork.BookTypes.GetAllAsync();

            }, nameof(GetAllAsync));
        }

        public async Task<ServiceResult<bool>> UpdateAsync(BookType bookType)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                if (!await _unitOfWork.BookTypes.ExistsAsync(bookType.Id))
                {
                    throw new NotFoundException(nameof(BookType), bookType.Id);
                }

                ValidationHelper.Validate(bookType);

                var existingBookType = await _unitOfWork.BookTypes.FindByNameAsync(bookType.Name);
                if (existingBookType != null && existingBookType.Id != bookType.Id)
                {
                    throw new AggregateValidationException($"Another book type with the name '{bookType.Name}' already exists.");
                }

                await _unitOfWork.BookTypes.UpdateAsync(bookType);
                await _unitOfWork.SaveChangesAsync();
                return true;

            }, nameof(UpdateAsync));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var bookType = await _unitOfWork.BookTypes.GetByIdAsync(id);
                if (bookType == null)
                {
                    throw new NotFoundException(nameof(BookType), id);
                }

                if (await _unitOfWork.BookTypes.HasEditionsAsync(id))
                {
                    throw new BusinessRuleException("Cannot delete a book type that has associated editions.");
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var success = await _unitOfWork.BookTypes.DeleteAsync(id);
                    if (!success)
                    {
                        throw new InvalidOperationException("Failed to delete book type");
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
                return await _unitOfWork.BookTypes.ExistsAsync(id);

            }, nameof(ExistsAsync));
        }
    }
}
