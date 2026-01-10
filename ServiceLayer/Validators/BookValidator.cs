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
    public class BookValidator : AbstractValidator<Book>
    {
        public BookValidator()
        {
            RuleFor(x => x.Domains)
                .NotEmpty()
                .WithMessage("Book must belong to at least one domain.");

            // Prevent ancestor-descendant relationships in domain assignments
            RuleFor(x => x.Domains)
                .Must(HaveValidDomainRelationships)
                .WithMessage("A book cannot be explicitly assigned to domains that are in an ancestor-descendant relationship.");
        }

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
