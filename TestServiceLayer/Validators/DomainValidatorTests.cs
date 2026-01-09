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
    public class DomainValidatorTests : ValidatorTestBase<DomainValidator, Domain>
    {
        [TestMethod]
        public void Validate_DomainWithValidParent_ShouldPassValidation()
        {
            // Arrange
            var parentDomain = _fixture.Build<Domain>()
                .Create();
            var domain = _fixture.Build<Domain>()
                .With(d => d.ParentDomain, parentDomain)
                .Create();

            // Act
            var result = Validate(domain);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void Validate_DomainWithNullParent_ShouldPassValidation()
        {
            // Arrange
            var domain = _fixture.Build<Domain>()
                .With(d => d.Name, "Root Domain")
                .Without(d => d.ParentDomain)
                .Create();

            // Act
            var result = Validate(domain);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void Validate_DomainWithSelfAsParent_ShouldFailValidation()
        {
            // Arrange
            var domain = _fixture.Build<Domain>()
                .Create();
            domain.ParentDomain = domain;

            // Act
            var result = Validate(domain);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("Parent domain cannot create a circular reference"));
        }

        [TestMethod]
        public void Validate_DomainWithParentBeingDescendant_ShouldFailValidation()
        {
            // Arrange
            var rootDomain = _fixture.Build<Domain>()
                                .Create();
            var childDomain = _fixture.Build<Domain>()
                                .With(d => d.ParentDomain, rootDomain)
                                .Create();
            var grandchildDomain = _fixture.Build<Domain>()
                                .With(d => d.ParentDomain, childDomain)
                                .Create();

            rootDomain.ParentDomain = grandchildDomain;

            // Act
            var result = Validate(rootDomain);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("Parent domain cannot create a circular reference"));
        }
    }
}
