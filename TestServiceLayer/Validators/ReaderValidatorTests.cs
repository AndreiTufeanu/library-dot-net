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
    public class ReaderValidatorTests : ValidatorTestBase<ReaderValidator, Reader>
    {
        [TestMethod]
        public void Validate_ReaderWithOnlyEmail_ShouldPassValidation()
        {
            // Arrange
            var reader = _fixture.Build<Reader>()
                .With(r => r.Email, "jane.smith@example.com")
                .Without(r => r.PhoneNumber)
                .With(r => r.DateOfBirth, DateTime.Now.AddYears(-25))
                .Create();

            // Act
            var result = Validate(reader);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void Validate_ReaderWithOnlyPhone_ShouldPassValidation()
        {
            // Arrange
            var reader = _fixture.Build<Reader>()
                .With(r => r.PhoneNumber, "+9876543210")
                .Without(r => r.Email)
                .With(r => r.DateOfBirth, DateTime.Now.AddYears(-30))
                .Create();

            // Act
            var result = Validate(reader);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void Validate_ReaderWithNoContactInfo_ShouldFailValidation()
        {
            // Arrange
            var reader = _fixture.Build<Reader>()
                .Without(r => r.Email)
                .Without(r => r.PhoneNumber)
                .With(r => r.DateOfBirth, DateTime.Now.AddYears(-18))
                .Create();

            // Act
            var result = Validate(reader);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("At least one contact method (phone or email) must be provided"));
        }

        [TestMethod]
        public void Validate_ReaderWithEmptyContactInfo_ShouldFailValidation()
        {
            // Arrange
            var reader = _fixture.Build<Reader>()
                .With(r => r.Email, "")
                .With(r => r.PhoneNumber, "   ")
                .With(r => r.DateOfBirth, DateTime.Now.AddYears(-18))
                .Create();

            // Act
            var result = Validate(reader);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("At least one contact method (phone or email) must be provided"));
        }

        [TestMethod]
        public void Validate_ReaderUnder14YearsOld_ShouldFailValidation()
        {
            // Arrange
            var reader = _fixture.Build<Reader>()
                .With(r => r.Email, "young@example.com")
                .With(r => r.PhoneNumber, "+1111111111")
                .With(r => r.DateOfBirth, DateTime.Now.AddYears(-13).AddDays(-364))
                .Create();

            // Act
            var result = Validate(reader);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("Reader must be at least 14 years old"));
        }

        [TestMethod]
        public void Validate_ReaderExactly14YearsOld_ShouldPassValidation()
        {
            // Arrange
            var reader = _fixture.Build<Reader>()
                .With(r => r.Email, "exactly14@example.com")
                .With(r => r.PhoneNumber, "+2222222222")
                .With(r => r.DateOfBirth, DateTime.Now.AddYears(-14))
                .Create();

            // Act
            var result = Validate(reader);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void Validate_ReaderOver14YearsOld_ShouldPassValidation()
        {
            // Arrange
            var reader = _fixture.Build<Reader>()
                .With(r => r.Email, "adult@example.com")
                .With(r => r.PhoneNumber, "+3333333333")
                .With(r => r.DateOfBirth, DateTime.Now.AddYears(-20))
                .Create();

            // Act
            var result = Validate(reader);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void Validate_ReaderWithValidDataAndBothContacts_ShouldPassValidation()
        {
            // Arrange
            var reader = _fixture.Build<Reader>()
                .With(r => r.FirstName, "Complete")
                .With(r => r.LastName, "Reader")
                .With(r => r.Address, "123 Main St, City, Country")
                .With(r => r.Email, "complete@example.com")
                .With(r => r.PhoneNumber, "+4444444444")
                .With(r => r.DateOfBirth, DateTime.Now.AddYears(-35))
                .Create();

            // Act
            var result = Validate(reader);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}
