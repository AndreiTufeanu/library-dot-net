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
    /// <summary>Provides service operations for managing <see cref="Author"/> entities in the library management system.</summary>
    /// <remarks>
    /// Implements <see cref="IAuthorService"/> to provide CRUD operations with business rule validation.
    /// All operations are performed asynchronously and return <see cref="ServiceResult{T}"/> objects.
    /// </remarks>
    public class AuthorService : BaseService, IAuthorService
    {
        /// <summary>The unit of work instance for coordinating repository operations and transactions.</summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>Initializes a new instance of the <see cref="AuthorService"/> class.</summary>
        /// <param name="unitOfWork">The unit of work instance for coordinating repository operations.</param>
        /// <param name="logger">The logger instance for logging service operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="unitOfWork"/> is null.</exception>
        public AuthorService(
            IUnitOfWork unitOfWork,
            ILogger<AuthorService> logger)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <inheritdoc/>
        /// <exception cref="AggregateValidationException"></exception>
        public async Task<ServiceResult<Author>> CreateAsync(Author author)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {   
                ValidationHelper.Validate(author);

                var existingAuthor = await _unitOfWork.Authors.FindByNameAsync(
                    author.FirstName,
                    author.LastName);

                if (existingAuthor != null)
                {
                    throw new AggregateValidationException($"Author '{author.FirstName} {author.LastName}' already exists.");
                }

                var addedAuthor = await _unitOfWork.Authors.AddAsync(author);
                await _unitOfWork.SaveChangesAsync();

                return addedAuthor;

            }, nameof(CreateAsync));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
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

        /// <inheritdoc/>
        public async Task<ServiceResult<IEnumerable<Author>>> GetAllAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Authors.GetAllAsync();

            }, nameof(GetAllAsync));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="AggregateValidationException"></exception>
        public async Task<ServiceResult<bool>> UpdateAsync(Author author)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                if (!await _unitOfWork.Authors.ExistsAsync(author.Id))
                {
                    throw new NotFoundException(nameof(Author), author.Id);
                }

                ValidationHelper.Validate(author);

                var existingAuthor = await _unitOfWork.Authors.FindByNameAsync(
                    author.FirstName,
                    author.LastName);

                if (existingAuthor != null && existingAuthor.Id != author.Id)
                {
                    throw new AggregateValidationException($"Another author with name '{author.FirstName} {author.LastName}' already exists.");
                }

                await _unitOfWork.Authors.UpdateAsync(author);
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
                var author = await _unitOfWork.Authors.GetByIdAsync(id);
                if (author == null)
                {
                    throw new NotFoundException(nameof(Author), id);
                }

                if (await _unitOfWork.Authors.HasBooksAsync(id))
                {
                    throw new BusinessRuleException("Cannot delete an author that has associated books.");
                }

                var success = await _unitOfWork.Authors.DeleteAsync(id);
                if (!success)
                    throw new InvalidOperationException("Failed to delete author");

                await _unitOfWork.SaveChangesAsync();
                return true;

            }, nameof(DeleteAsync));
        }

        /// <inheritdoc/>
        public async Task<ServiceResult<bool>> ExistsAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Authors.ExistsAsync(id);

            }, nameof(ExistsAsync));
        }
    }
}
