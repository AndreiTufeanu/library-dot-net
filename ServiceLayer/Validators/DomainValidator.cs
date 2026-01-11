using DomainModel.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Validators
{
    /// <summary>
    /// Validator for <see cref="Domain"/> entities that enforces business rules
    /// related to domain hierarchy and relationships.
    /// </summary>
    public class DomainValidator : AbstractValidator<Domain>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainValidator"/> class
        /// and configures the validation rules.
        /// </summary>
        public DomainValidator()
        {
            // Prevent circular reference in parent-child hierarchy
            RuleFor(x => x.ParentDomain)
                .Must((domain, parent) => !IsCircularReference(domain, parent))
                .WithMessage("Parent domain cannot create a circular reference.");
        }

        /// <summary>
        /// Checks if setting a parent domain would create a circular reference.
        /// </summary>
        /// <param name="domain">The domain being validated.</param>
        /// <param name="parent">The proposed parent domain.</param>
        /// <returns>
        /// <c>true</c> if setting the parent would create a circular reference;
        /// otherwise, <c>false</c>.
        /// </returns>
        private bool IsCircularReference(Domain domain, Domain parent)
        {
            if (parent == null || domain == null)
                return false;

            if (parent.Id == domain.Id)
                return true;

            var current = parent.ParentDomain;
            while (current != null)
            {
                if (current.Id == domain.Id)
                    return true;
                current = current.ParentDomain;
            }

            return false;
        }
    }
}
