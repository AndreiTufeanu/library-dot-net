using AutoFixture;
using DomainModel.Entities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLayer.Validators;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServiceLayer.Validators
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class BookValidatorTests : ValidatorTestBase<BookValidator, Book>
    {
        [TestMethod]
        public void Validate_BookWithValidDomains_ShouldPassValidation()
        {
            // Arrange
            var book = _fixture.Build<Book>()
                .FromFactory(() => new Book(initialCopies: 10))
                .With(b => b.Title, "Test Book")
                .Create();

            var domain1 = new Domain { Id = Guid.NewGuid(), Name = "Science Fiction" };
            var domain2 = new Domain { Id = Guid.NewGuid(), Name = "Fantasy" };
            book.Domains.Add(domain1);
            book.Domains.Add(domain2);

            // Act
            var result = Validate(book);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void Validate_BookWithNoDomains_ShouldFailValidation()
        {
            // Arrange
            var book = _fixture.Build<Book>()
                .FromFactory(() => new Book(initialCopies: 10))
                .With(b => b.Title, "Test Book")
                .Create();

            // Act
            var result = Validate(book);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Book must belong to at least one domain"));
        }

        [TestMethod]
        public void Validate_BookWithEmptyDomainCollection_ShouldFailValidation()
        {
            // Arrange
            var book = _fixture.Build<Book>()
                .FromFactory(() => new Book(initialCopies: 10))
                .With(b => b.Title, "Test Book")
                .Create();

            // Act
            var result = Validate(book);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Book must belong to at least one domain"));
        }

        [TestMethod]
        public void Validate_BookWithAncestorDescendantDomains_ShouldFailValidation()
        {
            // Arrange
            var book = _fixture.Build<Book>()
                .FromFactory(() => new Book(initialCopies: 10))
                .With(b => b.Title, "Test Book")
                .Create();

            var parentDomain = new Domain
            {
                Id = Guid.NewGuid(),
                Name = "Science",
            };

            var childDomain = new Domain
            {
                Id = Guid.NewGuid(),
                Name = "Physics",
                ParentDomain = parentDomain
            };

            parentDomain.Subdomains.Add(childDomain);

            book.Domains.Add(parentDomain);
            book.Domains.Add(childDomain);

            // Act
            var result = Validate(book);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("cannot be explicitly assigned to domains that are in an ancestor-descendant relationship"));
        }
    }
}
