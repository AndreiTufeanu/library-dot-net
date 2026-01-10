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
    public class DomainService : BaseService, IDomainService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<Domain> _validator;

        public DomainService(
            IUnitOfWork unitOfWork,
            IValidator<Domain> validator,
            ILogger<DomainService> logger)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

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

        public async Task<ServiceResult<IEnumerable<Domain>>> GetAllAsync()
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Domains.GetAllAsync();

            }, nameof(GetAllAsync));
        }

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

        public async Task<ServiceResult<bool>> ExistsAsync(Guid id)
        {
            return await ExecuteServiceOperationAsync(async () =>
            {
                return await _unitOfWork.Domains.ExistsAsync(id);

            }, nameof(ExistsAsync));
        }
    }
}
