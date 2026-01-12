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
    /// <summary>Provides service operations for managing <see cref="Domain"/> entities in the library management system.</summary>
    /// <remarks>
    /// Implements <see cref="IDomainService"/> to handle hierarchical domain/category management.
    /// Enforces uniqueness of domain names and prevents circular parent-child relationships.
    /// </remarks>
    public class DomainService : BaseService, IDomainService
    {
        /// <summary>The unit of work instance for coordinating repository operations and transactions.</summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>The validator instance for enforcing domain-specific business rules.</summary>
        private readonly IValidator<Domain> _validator;

        /// <summary>Initializes a new instance of the <see cref="DomainService"/> class.</summary>
        /// <param name="unitOfWork">The unit of work instance for coordinating repository operations.</param>
        /// <param name="validator">The validator instance for enforcing domain-specific business rules.</param>
        /// <param name="logger">The logger instance for logging service operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="unitOfWork"/> or <paramref name="validator"/> is null.</exception>
        public DomainService(
            IUnitOfWork unitOfWork,
            IValidator<Domain> validator,
            ILogger<DomainService> logger)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <inheritdoc/>
        /// <exception cref="AggregateValidationException"></exception>
        /// <exception cref="NotFoundException"></exception>
        public async Task<ServiceResult<Domain>> CreateAsync(Domain domain)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                ValidationHelper.Validate(domain, _validator);

                var existingDomain = await _unitOfWork.Domains.FindByNameAsync(domain.Name);
                if (existingDomain != null)
                {
                    throw new AggregateValidationException($"Domain '{domain.Name}' already exists.");
                }

                if (domain.ParentDomain != null && domain.ParentDomain.Id != Guid.Empty)
                {
                    var parent = await _unitOfWork.Domains.GetByIdAsync(domain.ParentDomain.Id);
                    if (parent == null)
                    {
                        throw new NotFoundException(nameof(Domain), domain.ParentDomain.Id);
                    }
                }

                var addedDomain = await _unitOfWork.Domains.AddAsync(domain);
                await _unitOfWork.SaveChangesAsync();

                return addedDomain;

            }, nameof(CreateAsync));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        public async Task<ServiceResult<Domain>> GetByIdAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                var domain = await _unitOfWork.Domains.GetByIdAsync(id);
                if (domain == null)
                {
                    throw new NotFoundException(nameof(Domain), id);
                }
                return domain;

            }, nameof(GetByIdAsync));
        }

        /// <inheritdoc/>
        public async Task<ServiceResult<IEnumerable<Domain>>> GetAllAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Domains.GetAllAsync();

            }, nameof(GetAllAsync));
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="AggregateValidationException"></exception>
        public async Task<ServiceResult<bool>> UpdateAsync(Domain domain)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                if (!await _unitOfWork.Domains.ExistsAsync(domain.Id))
                {
                    throw new NotFoundException(nameof(Domain), domain.Id);
                }

                ValidationHelper.Validate(domain, _validator);

                var existingDomain = await _unitOfWork.Domains.FindByNameAsync(domain.Name);
                if (existingDomain != null && existingDomain.Id != domain.Id)
                {
                    throw new AggregateValidationException($"Another domain with name '{domain.Name}' already exists.");
                }

                await _unitOfWork.Domains.UpdateAsync(domain);
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
                var domain = await _unitOfWork.Domains.GetByIdAsync(id);
                if (domain == null)
                {
                    throw new NotFoundException(nameof(Domain), id);
                }

                if (await _unitOfWork.Domains.HasBooksAsync(id))
                {
                    throw new BusinessRuleException("Cannot delete a domain that has associated books.");
                }

                if (await _unitOfWork.Domains.HasSubdomainsAsync(id))
                {
                    throw new BusinessRuleException("Cannot delete a domain that has subdomains. Delete subdomains first.");
                }

                var success = await _unitOfWork.Domains.DeleteAsync(id);
                if (!success)
                {
                    throw new InvalidOperationException("Failed to delete domain");
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
                return await _unitOfWork.Domains.ExistsAsync(id);

            }, nameof(ExistsAsync));
        }
    }
}
