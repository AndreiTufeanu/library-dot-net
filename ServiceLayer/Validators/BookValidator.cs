using DomainModel.Entities;
using FluentValidation;
using ServiceLayer.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Validators
{
    /// <summary>
    /// Validator for <see cref="Book"/> entities that enforces business rules
    /// related to book domain assignments and relationships.
    /// </summary>
    public class BookValidator : AbstractValidator<Book>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BookValidator"/> class
        /// and configures the validation rules.
        /// </summary>
        public BookValidator()
        {
            // Ensure at least one domain is assigned to the book
            RuleFor(x => x.Domains)
                .NotEmpty()
                .WithMessage("Book must belong to at least one domain.");

            // Prevent ancestor-descendant relationships in domain assignments
            RuleFor(x => x.Domains)
                .Must(HaveValidDomainRelationships)
                .WithMessage("A book cannot be explicitly assigned to domains that are in an ancestor-descendant relationship.");
        }

        /// <summary>
        /// Validates that no two domains in the collection are in an ancestor-descendant relationship.
        /// </summary>
        /// <param name="domains">The collection of domains to validate.</param>
        /// <returns>
        /// <c>true</c> if no domains in the collection are in an ancestor-descendant relationship;
        /// otherwise, <c>false</c>.
        /// </returns>
        private bool HaveValidDomainRelationships(ICollection<Domain> domains)
        {
            if (domains.Count <= 1)
                return true;

            var domainList = domains.ToList();
            for (int i = 0; i < domainList.Count; i++)
            {
                for (int j = i + 1; j < domainList.Count; j++)
                {
                    var domain1 = domainList[i];
                    var domain2 = domainList[j];

                    if (domain1.IsAncestor(domain2) || domain1.IsDescendant(domain2))
                        return false;
                }
            }
            return true;
        }
    }
}
