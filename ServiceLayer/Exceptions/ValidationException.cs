using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Exceptions
{
    /// <summary>
    /// Represents an exception that occurs when input validation fails.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This exception is thrown when data provided to a service method fails validation.
    /// Validation can include DataAnnotations validation, FluentValidation rules, or
    /// custom business logic validation that doesn't rise to the level of a
    /// <see cref="BusinessRuleException"/>.
    /// </para>
    /// <para>
    /// This exception contains a collection of validation error messages, allowing
    /// callers to receive multiple validation failures in a single exception.
    /// </para>
    /// <para>
    /// Unlike <see cref="BusinessRuleException"/>, which represents violations of
    /// complex business logic, <see cref="ValidationException"/> is typically used
    /// for input data validation (required fields, format validation, range checks, etc.).
    /// </para>
    /// </remarks>
    public class ValidationException : Exception
    {
        /// <summary>
        /// Gets the collection of validation error messages.
        /// </summary>
        /// <value>A list of strings describing each validation failure.</value>
        public List<string> Errors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with multiple validation errors.
        /// </summary>
        /// <param name="errors">The collection of validation error messages.</param>
        public ValidationException(List<string> errors)
            : base($"Validation failed: {string.Join("; ", errors)}")
        {
            Errors = errors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a single validation error.
        /// </summary>
        /// <param name="error">The validation error message.</param>
        public ValidationException(string error)
            : base($"Validation failed: {error}")
        {
            Errors = new List<string> { error };
        }
    }
}
