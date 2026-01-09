using AutoFixture;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServiceLayer.Validators
{
    [ExcludeFromCodeCoverage]
    public abstract class ValidatorTestBase<TValidator, TEntity>
            where TValidator : AbstractValidator<TEntity>, new()
            where TEntity : class
    {
        protected readonly Fixture _fixture;
        protected readonly TValidator _validator;

        protected ValidatorTestBase()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _validator = new TValidator();
        }

        protected FluentValidation.Results.ValidationResult Validate(TEntity entity)
        {
            return _validator.Validate(entity);
        }
    }
}
