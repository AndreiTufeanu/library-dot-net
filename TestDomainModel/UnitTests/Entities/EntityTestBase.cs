using AutoFixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDomainModel.UnitTests.Entities
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public abstract class EntityTestBase<T> where T : class
    {
        protected readonly Fixture _fixture;

        protected EntityTestBase()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        protected ValidationResult[] ValidateEntity(T entity)
        {
            var validationContext = new ValidationContext(entity);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(entity, validationContext, validationResults, true);
            return validationResults.ToArray();
        }
    }
}
