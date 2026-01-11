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
    /// <summary>
    /// Provides a base implementation for service classes with common functionality
    /// such as error handling, logging, and service result management.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This abstract class implements the Template Method pattern to provide consistent
    /// error handling and logging across all service operations. It ensures that all
    /// service methods follow the same pattern for success/failure responses and
    /// properly log exceptions.
    /// </para>
    /// <para>
    /// Derived service classes should use the <see cref="ExecuteServiceOperationAsync{TResult}"/>
    /// method to wrap their business logic, which will automatically handle exceptions,
    /// logging, and service result creation.
    /// </para>
    /// </remarks>
    public abstract class BaseService
    {

        /// <summary>
        /// The logger instance for logging service operations.
        /// </summary>
        /// <remarks>
        /// This logger is specific to the derived service class and provides
        /// contextual logging with the service type name.
        /// </remarks>
        protected readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for the derived service class.</param>
        /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
        protected BaseService(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes a service operation with standardized error handling, logging, and service result creation.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the operation.</typeparam>
        /// <param name="operation">The asynchronous function representing the service operation to execute.</param>
        /// <param name="operationName">The name of the operation for logging purposes.</param>
        /// <returns>
        /// A <see cref="ServiceResult{TResult}"/> containing either the successful result
        /// or error information if the operation failed.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The method handles the following exception types:
        /// <list type="bullet">
        /// <item><description><see cref="AggregateValidationException"/>: Returns validation failure result</description></item>
        /// <item><description><see cref="NotFoundException"/>: Returns not found failure result</description></item>
        /// <item><description><see cref="BusinessRuleException"/>: Returns business rule violation result</description></item>
        /// <item><description><see cref="Exception"/>: Returns generic error result</description></item>
        /// </list>
        /// </para>
        /// </remarks>
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
            catch (AggregateValidationException ex)
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
