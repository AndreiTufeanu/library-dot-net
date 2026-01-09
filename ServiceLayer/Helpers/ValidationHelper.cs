using FluentValidation;
using ServiceLayer.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Helpers
{
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates an object using Data Annotations and FluentValidation
        /// Throws AggregateValidationException if validation fails
        /// </summary>
        public static void Validate<T>(T obj, IValidator<T> validator = null)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var errors = new List<string>();

            // Data Annotations validation
            var validationContext = new ValidationContext(obj);
            var validationResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(obj, validationContext, validationResults, true))
            {
                errors.AddRange(validationResults
                    .Select(vr => vr.ErrorMessage)
                    .Where(error => !string.IsNullOrWhiteSpace(error)));
            }

            // FluentValidation (if validator provided)
            if (validator != null)
            {
                var fluentResult = validator.Validate(obj);
                if (!fluentResult.IsValid)
                {
                    errors.AddRange(fluentResult.Errors
                        .Select(e => e.ErrorMessage)
                        .Where(error => !string.IsNullOrWhiteSpace(error)));
                }
            }

            // Throw if any errors
            if (errors.Any())
            {
                throw new AggregateValidationException(errors);
            }
        }
    }
}
