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
    public class AuthorService : BaseService, IAuthorService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthorService(
            IUnitOfWork unitOfWork,
            IValidator<Author> validator,
            ILogger<AuthorService> logger)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<ServiceResult<Author>> CreateAsync(Author author)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                ValidationHelper.Validate(author);

                // Check if author with same name already exists
                var existingAuthor = await _unitOfWork.Authors.FindByNameAsync(
                    author.FirstName,
                    author.LastName);

                if (existingAuthor != null)
                {
                    throw new Exceptions.ValidationException($"Author '{author.FirstName} {author.LastName}' already exists.");
                }

                var addedAuthor = await _unitOfWork.Authors.AddAsync(author);
                await _unitOfWork.SaveChangesAsync();

                return addedAuthor;

            }, nameof(CreateAsync));
        }

        public async Task<ServiceResult<Author>> GetByIdAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var author = await _unitOfWork.Authors.GetByIdAsync(id);
                if (author == null)
                {
                    throw new NotFoundException(nameof(Author), id);
                }
                return author;

            }, nameof(GetByIdAsync));
        }

        public async Task<ServiceResult<IEnumerable<Author>>> GetAllAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Authors.GetAllAsync();

            }, nameof(GetAllAsync));
        }

        public async Task<ServiceResult<bool>> UpdateAsync(Author author)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                if (!await _unitOfWork.Authors.ExistsAsync(author.Id))
                {
                    throw new NotFoundException(nameof(Author), author.Id);
                }

                ValidationHelper.Validate(author);

                // Check if another author with same name exists
                var existingAuthor = await _unitOfWork.Authors.FindByNameAsync(
                    author.FirstName,
                    author.LastName);

                if (existingAuthor != null && existingAuthor.Id != author.Id)
                {
                    throw new Exceptions.ValidationException($"Another author with name '{author.FirstName} {author.LastName}' already exists.");
                }

                await _unitOfWork.Authors.UpdateAsync(author);
                await _unitOfWork.SaveChangesAsync();
                return true;

            }, nameof(UpdateAsync));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var author = await _unitOfWork.Authors.GetByIdAsync(id);
                if (author == null)
                {
                    throw new NotFoundException(nameof(Author), id);
                }

                // Check if author has books before deletion
                if (await _unitOfWork.Authors.HasBooksAsync(id))
                {
                    throw new BusinessRuleException("Cannot delete an author that has associated books.");
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var success = await _unitOfWork.Authors.DeleteAsync(id);
                    if (!success)
                    {
                        throw new InvalidOperationException("Failed to delete author");
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
                return await _unitOfWork.Authors.ExistsAsync(id);

            }, nameof(ExistsAsync));
        }

        public async Task<ServiceResult<Author>> FindByNameAsync(string firstName, string lastName)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(firstName))
                {
                    throw new Exceptions.ValidationException("First name cannot be empty.");
                }

                if (string.IsNullOrWhiteSpace(lastName))
                {
                    throw new Exceptions.ValidationException("Last name cannot be empty.");
                }

                var author = await _unitOfWork.Authors.FindByNameAsync(firstName, lastName);
                if (author == null)
                {
                    throw new NotFoundException($"Author '{firstName} {lastName}' not found.");
                }

                return author;

            }, nameof(FindByNameAsync));
        }

        public async Task<ServiceResult<IEnumerable<Author>>> FindByLastNameAsync(string lastName)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(lastName))
                {
                    throw new Exceptions.ValidationException("Last name cannot be empty.");
                }

                return await _unitOfWork.Authors.FindByLastNameAsync(lastName);

            }, nameof(FindByLastNameAsync));
        }
    }
}
