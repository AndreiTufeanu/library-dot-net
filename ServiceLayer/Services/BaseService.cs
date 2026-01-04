using ServiceLayer.Exceptions;
using ServiceLayer.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ServiceLayer.Services
{
    public abstract class BaseService
    {
        protected readonly ILogger _logger;

        protected BaseService(ILogger logger)
        {
            _logger = logger;
        }

        protected async Task<ServiceResult<TResult>> ExecuteServiceOperationAsync<TResult>(
            Func<Task<TResult>> operation,
            string operationName)
        {
            try
            {
                _logger.LogInformation("Starting {OperationName}", operationName);

                var result = await operation();

                _logger.LogInformation("Completed {OperationName} successfully", operationName);
                return ServiceResult<TResult>.SuccessResult(result);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation failed for {OperationName}: {ErrorMessage}",
                    operationName, ex.Message);
                return ServiceResult<TResult>.ValidationFailed(ex.Errors);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Resource not found for {OperationName}: {ErrorMessage}",
                    operationName, ex.Message);
                return ServiceResult<TResult>.FailureResult(ex.Message);
            }
            catch (BusinessRuleException ex)
            {
                _logger.LogWarning("Business rule violation for {OperationName}: {ErrorMessage}",
                    operationName, ex.Message);
                return ServiceResult<TResult>.FailureResult(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during {OperationName}", operationName);
                return ServiceResult<TResult>.FailureResult($"An error occurred: {ex.Message}");
            }
        }
    }
}
