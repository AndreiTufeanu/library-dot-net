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
    public class EditionValidatorTests : ValidatorTestBase<EditionValidator, Edition>
    {
        [TestMethod]
        public void Validate_EditionWithPastPublicationDate_ShouldPassValidation()
        {
            // Arrange
            var edition = _fixture.Build<Edition>()
                .With(e => e.NumberOfPages, 300)
                .With(e => e.PublicationDate, DateTime.Now.AddYears(-1))
                .Create();

            // Act
            var result = Validate(edition);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void Validate_EditionWithCurrentDate_ShouldPassValidation()
        {
            // Arrange
            var edition = _fixture.Build<Edition>()
                .With(e => e.NumberOfPages, 300)
                .With(e => e.PublicationDate, DateTime.Now.Date)
                .Create();

            // Act
            var result = Validate(edition);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void Validate_EditionWithFuturePublicationDate_ShouldFailValidation()
        {
            // Arrange
            var edition = _fixture.Build<Edition>()
                .With(e => e.NumberOfPages, 300)
                .With(e => e.PublicationDate, DateTime.Now.AddDays(1))
                .Create();

            // Act
            var result = Validate(edition);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("Publication date cannot be in the future"));
        }

        [TestMethod]
        public void Validate_EditionWithFarFuturePublicationDate_ShouldFailValidation()
        {
            // Arrange
            var edition = _fixture.Build<Edition>()
                .With(e => e.NumberOfPages, 300)
                .With(e => e.PublicationDate, DateTime.Now.AddYears(10))
                .Create();

            // Act
            var result = Validate(edition);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("Publication date cannot be in the future"));
        }

        [TestMethod]
        public void Validate_EditionWithMinimumValidPageCount_ShouldPassValidation()
        {
            // Arrange
            var edition = _fixture.Build<Edition>()
                .With(e => e.NumberOfPages, 30)
                .With(e => e.PublicationDate, DateTime.Now.AddYears(-1))
                .Create();

            // Act
            var result = Validate(edition);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}
