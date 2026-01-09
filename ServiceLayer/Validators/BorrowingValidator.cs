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
    public class BorrowingValidator : AbstractValidator<Borrowing>
    {
        public BorrowingValidator()
        {
            // Due date must be after borrow date
            RuleFor(x => x.DueDate)
                .GreaterThan(x => x.BorrowDate)
                .WithMessage("Due date must be after borrow date.");

            // Return date must be on or after borrow date (if provided)
            RuleFor(x => x.ReturnDate)
                .GreaterThanOrEqualTo(x => x.BorrowDate)
                .When(x => x.ReturnDate.HasValue)
                .WithMessage("Return date must be on or after borrow date.");

            // Extension days cannot be negative (if provided)
            RuleFor(x => x.ExtensionDays)
                .GreaterThanOrEqualTo(0)
                .When(x => x.ExtensionDays.HasValue)
                .WithMessage("Extension days cannot be negative.");

            // At least one book copy must be borrowed
            RuleFor(x => x.BookCopies)
                .NotEmpty()
                .WithMessage("At least one book copy must be borrowed.");

            // Can't borrow multiple copies/editions of the same book title
            RuleFor(x => x.BookCopies)
                .Must(HaveDistinctBookTitles)
                .WithMessage("Cannot borrow multiple copies/editions of the same book title in one transaction.");

            // If borrowing 3+ books, must have at least 2 distinct domains
            RuleFor(x => x.BookCopies)
                .Must(HaveSufficientDomainDiversity)
                .WithMessage("When borrowing 3 or more books, they must belong to at least 2 distinct domains.");
        }

        private bool HaveDistinctBookTitles(ICollection<BookCopy> bookCopies)
        {
            if (bookCopies == null || bookCopies.Count <= 1)
                return true;

            var distinctBookCount = bookCopies
                .Select(bc => bc.Edition?.Book?.Id)
                .Where(id => id.HasValue)
                .Distinct()
                .Count();

            return distinctBookCount == bookCopies.Count;
        }

        private bool HaveSufficientDomainDiversity(ICollection<BookCopy> bookCopies)
        {
            if (bookCopies == null || bookCopies.Count < 3)
                return true;

            var domainIds = new HashSet<Guid>();

            foreach (var copy in bookCopies)
            {
                var edition = copy.Edition;
                if (edition?.Book?.Domains != null)
                {
                    foreach (var domain in edition.Book.Domains)
                    {
                        domainIds.Add(domain.Id);
                    }
                }
            }

            return domainIds.Count >= 2;
        }
    }
}
