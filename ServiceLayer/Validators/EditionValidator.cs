using DomainModel.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Validators
{
    public class EditionValidator : AbstractValidator<Edition>
    {
        public EditionValidator()
        {
            // Publication date cannot be in the future
            RuleFor(x => x.PublicationDate)
                .LessThanOrEqualTo(DateTime.Now)
                .WithMessage("Publication date cannot be in the future.");
        }
    }
}
