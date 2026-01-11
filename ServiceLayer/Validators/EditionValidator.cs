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
    /// Validator for <see cref="Edition"/> entities that enforces business rules
    /// related to edition publication dates.
    /// </summary>
    public class EditionValidator : AbstractValidator<Edition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditionValidator"/> class
        /// and configures the validation rules.
        /// </summary>
        public EditionValidator()
        {
            // Publication date cannot be in the future
            RuleFor(x => x.PublicationDate)
                .LessThanOrEqualTo(DateTime.Now)
                .WithMessage("Publication date cannot be in the future.");
        }
    }
}
