using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using ServiceLayer.ServiceContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    public class BookTypeService : IBookTypeService
    {
        private readonly IBookTypeRepository _repository;
        private readonly IValidator<BookType> _validator;
        private readonly ILogger<BookTypeService> _logger;

        public BookTypeService(
            IBookTypeRepository repository,
            IValidator<BookType> validator,
            ILogger<BookTypeService> logger)
        {
            _repository = repository;
            _validator = validator;
            _logger = logger;
        }

        public ServiceResult<BookType> CreateBookType(BookType bookType)
        {
            try
            {
                _logger.LogInformation("Creating new book type: {Name}", bookType.Name);

                // Validate using Data Annotations
                var validationContext = new ValidationContext(bookType);
                var validationResults = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(bookType, validationContext, validationResults, true);

                if (!isValid)
                {
                    var errorMessages = validationResults.Select(vr => vr.ErrorMessage).ToList();
                    var combinedErrorMessage = string.Join("; ", errorMessages);

                    _logger.LogWarning("Data annotation validation failed for book type: {Errors}", combinedErrorMessage);
                    return ServiceResult<BookType>.FailureResult(combinedErrorMessage);
                }

                // Validate using FluentValidation
                var fluentValidationResult = _validator.Validate(bookType);
                if (!fluentValidationResult.IsValid)
                {
                    var errors = fluentValidationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    var combinedErrorMessage = string.Join("; ", errors);

                    _logger.LogWarning("FluentValidation failed for book type: {Errors}", combinedErrorMessage);
                    return ServiceResult<BookType>.FailureResult(combinedErrorMessage);
                }

                // Business logic: Check for duplicate names
                var existingBookTypes = _repository.GetAll();
                if (existingBookTypes.Any(bt =>
                    bt.Name.Equals(bookType.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.LogWarning("Duplicate book type name: {Name}", bookType.Name);
                    return ServiceResult<BookType>.FailureResult($"A book type with name '{bookType.Name}' already exists.");
                }

                // Add to repository
                _repository.Add(bookType);
                _repository.SaveChanges();

                _logger.LogInformation("Book type created successfully with ID: {Id}", bookType.Id);
                return ServiceResult<BookType>.SuccessResult(bookType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating book type: {Name}", bookType.Name);
                return ServiceResult<BookType>.FailureResult($"An error occurred while creating the book type: {ex.Message}");
            }
        }

        public ServiceResult<BookType> GetBookTypeById(Guid id)
        {
            try
            {
                _logger.LogInformation("Retrieving book type with ID: {Id}", id);

                var bookType = _repository.GetById(id);
                if (bookType == null)
                {
                    _logger.LogWarning("Book type not found with ID: {Id}", id);
                    return ServiceResult<BookType>.FailureResult($"Book type with ID '{id}' not found.");
                }

                _logger.LogInformation("Book type retrieved successfully: {Name}", bookType.Name);
                return ServiceResult<BookType>.SuccessResult(bookType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving book type with ID: {Id}", id);
                return ServiceResult<BookType>.FailureResult($"An error occurred while retrieving the book type: {ex.Message}");
            }
        }

        public ServiceResult<IEnumerable<BookType>> GetAllBookTypes()
        {
            try
            {
                _logger.LogInformation("Retrieving all book types");

                var bookTypes = _repository.GetAll();

                _logger.LogInformation("Retrieved {Count} book types", bookTypes.Count());
                return ServiceResult<IEnumerable<BookType>>.SuccessResult(bookTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all book types");
                return ServiceResult<IEnumerable<BookType>>.FailureResult($"An error occurred while retrieving book types: {ex.Message}");
            }
        }

        public ServiceResult<bool> UpdateBookType(BookType bookType)
        {
            try
            {
                _logger.LogInformation("Updating book type with ID: {Id}", bookType.Id);

                // Check if book type exists
                if (!_repository.Exists(bookType.Id))
                {
                    _logger.LogWarning("Book type not found with ID: {Id}", bookType.Id);
                    return ServiceResult<bool>.FailureResult($"Book type with ID '{bookType.Id}' not found.");
                }

                // Validate using Data Annotations
                var validationContext = new ValidationContext(bookType);
                var validationResults = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(bookType, validationContext, validationResults, true);

                if (!isValid)
                {
                    var errorMessages = validationResults.Select(vr => vr.ErrorMessage).ToList();
                    var combinedErrorMessage = string.Join("; ", errorMessages);

                    _logger.LogWarning("Data annotation validation failed for book type update: {Errors}", combinedErrorMessage);
                    return ServiceResult<bool>.FailureResult(combinedErrorMessage);
                }

                // Validate using FluentValidation
                var fluentValidationResult = _validator.Validate(bookType);
                if (!fluentValidationResult.IsValid)
                {
                    var errors = fluentValidationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    var combinedErrorMessage = string.Join("; ", errors);

                    _logger.LogWarning("FluentValidation failed for book type update: {Errors}", combinedErrorMessage);
                    return ServiceResult<bool>.FailureResult(combinedErrorMessage);
                }

                // Business logic: Check for duplicate names
                var existingBookTypes = _repository.GetAll();
                if (existingBookTypes.Any(bt =>
                    bt.Name.Equals(bookType.Name, StringComparison.OrdinalIgnoreCase) &&
                    bt.Id != bookType.Id))
                {
                    _logger.LogWarning("Duplicate book type name during update: {Name}", bookType.Name);
                    return ServiceResult<bool>.FailureResult($"Another book type with name '{bookType.Name}' already exists.");
                }

                // Update
                _repository.Update(bookType);
                _repository.SaveChanges();

                _logger.LogInformation("Book type updated successfully: {Name}", bookType.Name);
                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating book type with ID: {Id}", bookType.Id);
                return ServiceResult<bool>.FailureResult($"An error occurred while updating the book type: {ex.Message}");
            }
        }

        public ServiceResult<bool> DeleteBookType(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting book type with ID: {Id}", id);

                var bookType = _repository.GetById(id);
                if (bookType == null)
                {
                    _logger.LogWarning("Book type not found with ID: {Id}", id);
                    return ServiceResult<bool>.FailureResult($"Book type with ID '{id}' not found.");
                }

                _logger.LogInformation("Deleting book type: {Name} (ID: {Id})", bookType.Name, id);

                _repository.Delete(id);
                _repository.SaveChanges();

                _logger.LogInformation("Book type deleted successfully: {Name}", bookType.Name);
                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting book type with ID: {Id}", id);
                return ServiceResult<bool>.FailureResult($"An error occurred while deleting the book type: {ex.Message}");
            }
        }

        public ServiceResult<bool> BookTypeExists(Guid id)
        {
            try
            {
                var exists = _repository.Exists(id);
                _logger.LogDebug("Checked if book type exists with ID {Id}: {Exists}", id, exists);
                return ServiceResult<bool>.SuccessResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if book type exists with ID: {Id}", id);
                return ServiceResult<bool>.FailureResult($"An error occurred while checking book type existence: {ex.Message}");
            }
        }
    }
}
