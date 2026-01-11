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
    /// Validator for <see cref="Reader"/> entities that enforces business rules
    /// related to reader information and eligibility.
    /// </summary>
    public class ReaderValidator : AbstractValidator<Reader>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderValidator"/> class
        /// and configures the validation rules.
        /// </summary>
        public ReaderValidator()
        {
            // At least one contact method (phone or email)
            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.PhoneNumber) || !string.IsNullOrWhiteSpace(x.Email))
                .WithMessage("At least one contact method (phone or email) must be provided.");

            // Reader must be at least 14 years old
            RuleFor(x => x.DateOfBirth)
                .Must(dob => dob.AddYears(14) <= DateTime.Now)
                .WithMessage("Reader must be at least 14 years old.");
        }
    }
}
