using DomainModel.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Validators
{
    public class DomainValidator : AbstractValidator<Domain>
    {
        public DomainValidator()
        {
            // Prevent circular reference in parent-child hierarchy
            RuleFor(x => x.ParentDomain)
                .Must((domain, parent) => !IsCircularReference(domain, parent))
                .WithMessage("Parent domain cannot create a circular reference.");
        }

        private bool IsCircularReference(Domain domain, Domain parent)
        {
            if (parent == null || domain == null)
                return false;

            // Check if the parent is the domain itself
            if (parent.Id == domain.Id)
                return true;

            // Check if the parent is already a descendant of this domain
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
