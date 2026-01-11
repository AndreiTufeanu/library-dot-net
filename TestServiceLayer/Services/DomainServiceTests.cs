using AutoFixture;
using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLayer.Services;
using ServiceLayer.Validators;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServiceLayer.Services
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class DomainServiceTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<DomainService>> _loggerMock;
        private readonly IValidator<Domain> _validator;
        private readonly DomainService _service;

        private readonly Mock<IDomainRepository> _domainRepositoryMock;

        public DomainServiceTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<DomainService>>();

            _domainRepositoryMock = new Mock<IDomainRepository>();
            _unitOfWorkMock.Setup(u => u.Domains).Returns(_domainRepositoryMock.Object);

            _validator = new DomainValidator();
            _service = new DomainService(_unitOfWorkMock.Object, _validator, _loggerMock.Object);
        }

        private Domain CreateValidDomain()
        {
            return _fixture.Build<Domain>()
                .With(d => d.Name, "Science Fiction")
                .Create();
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WhenUnitOfWorkIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            IUnitOfWork unitOfWork = null;
            IValidator<Domain> validator = Mock.Of<IValidator<Domain>>();
            ILogger<DomainService> logger = Mock.Of<ILogger<DomainService>>();

            // Act
            Action act = () => new DomainService(unitOfWork, validator, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*unitOfWork*");
        }

        [TestMethod]
        public void Constructor_WhenValidatorIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var unitOfWork = Mock.Of<IUnitOfWork>();
            IValidator<Domain> validator = null;
            var logger = Mock.Of<ILogger<DomainService>>();

            // Act
            Action act = () => new DomainService(unitOfWork, validator, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*validator*");
        }

        [TestMethod]
        public void Constructor_WhenLoggerIsNull_ShouldThrow()
        {
            // Arrange
            var unitOfWork = Mock.Of<IUnitOfWork>();
            var validator = Mock.Of<IValidator<Domain>>();
            ILogger<DomainService> logger = null;

            // Act
            Action act = () => new DomainService(unitOfWork, validator, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        #endregion

        #region CreateAsync Tests

        [TestMethod]
        public async Task CreateAsync_ValidDomain_ShouldReturnSuccess()
        {
            // Arrange
            var domain = CreateValidDomain();
            var addedDomain = _fixture.Build<Domain>()
                .With(d => d.Id, domain.Id)
                .Create();

            _domainRepositoryMock.Setup(r => r.FindByNameAsync(domain.Name))
                .ReturnsAsync((Domain)null);
            _domainRepositoryMock.Setup(r => r.AddAsync(domain))
                .ReturnsAsync(addedDomain);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(domain);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be(addedDomain);
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.ValidationErrors.Should().BeEmpty();
        }

        [TestMethod]
        public async Task CreateAsync_ShouldCallRepositoryMethods()
        {
            // Arrange
            var domain = CreateValidDomain();

            _domainRepositoryMock.Setup(r => r.FindByNameAsync(domain.Name))
                .ReturnsAsync((Domain)null);
            _domainRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Domain>()))
                .ReturnsAsync(domain);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _service.CreateAsync(domain);

            // Assert
            _domainRepositoryMock.Verify(r => r.FindByNameAsync(domain.Name), Times.Once);
            _domainRepositoryMock.Verify(r => r.AddAsync(domain), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task CreateAsync_DuplicateDomain_ShouldReturnValidationError()
        {
            // Arrange
            var domain = CreateValidDomain();
            var existingDomain = CreateValidDomain();

            _domainRepositoryMock.Setup(r => r.FindByNameAsync(domain.Name))
                .ReturnsAsync(existingDomain);

            // Act
            var result = await _service.CreateAsync(domain);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain($"Domain '{domain.Name}' already exists");
        }

        [TestMethod]
        public async Task CreateAsync_WithValidParentDomain_ShouldReturnSuccess()
        {
            // Arrange
            var parentDomain = CreateValidDomain();
            var domain = CreateValidDomain();
            domain.ParentDomain = parentDomain;

            _domainRepositoryMock.Setup(r => r.FindByNameAsync(domain.Name))
                .ReturnsAsync((Domain)null);
            _domainRepositoryMock.Setup(r => r.GetByIdAsync(parentDomain.Id))
                .ReturnsAsync(parentDomain);
            _domainRepositoryMock.Setup(r => r.AddAsync(domain))
                .ReturnsAsync(domain);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(domain);

            // Assert
            result.Success.Should().BeTrue();
        }

        [TestMethod]
        public async Task CreateAsync_WithNonExistentParentDomain_ShouldReturnNotFoundError()
        {
            // Arrange
            var parentDomain = CreateValidDomain();
            var domain = CreateValidDomain();
            domain.ParentDomain = parentDomain;

            _domainRepositoryMock.Setup(r => r.FindByNameAsync(domain.Name))
                .ReturnsAsync((Domain)null);
            _domainRepositoryMock.Setup(r => r.GetByIdAsync(parentDomain.Id))
                .ReturnsAsync((Domain)null);

            // Act
            var result = await _service.CreateAsync(domain);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Domain with ID '{parentDomain.Id}' not found");
        }

        [TestMethod]
        public async Task CreateAsync_ValidationFailure_ShouldReturnValidationErrors()
        {
            // Arrange
            var domain = CreateValidDomain();
            domain.Name = "A";

            // Act
            var result = await _service.CreateAsync(domain);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().Contain(item => item.Contains("Domain name must be between"));
        }

        #endregion

        #region GetByIdAsync Tests

        [TestMethod]
        public async Task GetByIdAsync_DomainExists_ShouldReturnDomain()
        {
            // Arrange
            var domainId = Guid.NewGuid();
            var domain = _fixture.Build<Domain>()
                .With(d => d.Id, domainId)
                .Create();

            _domainRepositoryMock.Setup(r => r.GetByIdAsync(domainId))
                .ReturnsAsync(domain);

            // Act
            var result = await _service.GetByIdAsync(domainId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be(domain);
        }

        [TestMethod]
        public async Task GetByIdAsync_DomainNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var domainId = Guid.NewGuid();
            _domainRepositoryMock.Setup(r => r.GetByIdAsync(domainId))
                .ReturnsAsync((Domain)null);

            // Act
            var result = await _service.GetByIdAsync(domainId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Domain with ID '{domainId}' not found");
        }

        #endregion

        #region GetAllAsync Tests

        [TestMethod]
        public async Task GetAllAsync_DomainsExist_ShouldReturnAllDomains()
        {
            // Arrange
            var domains = _fixture.CreateMany<Domain>(3).ToList();
            _domainRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(domains);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(domains);
        }

        [TestMethod]
        public async Task GetAllAsync_NoDomains_ShouldReturnEmptyList()
        {
            // Arrange
            _domainRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Domain>());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region UpdateAsync Tests

        [TestMethod]
        public async Task UpdateAsync_ValidDomain_ShouldReturnSuccess()
        {
            // Arrange
            var domain = CreateValidDomain();

            _domainRepositoryMock.Setup(r => r.ExistsAsync(domain.Id))
                .ReturnsAsync(true);
            _domainRepositoryMock.Setup(r => r.FindByNameAsync(domain.Name))
                .ReturnsAsync((Domain)null);
            _domainRepositoryMock.Setup(r => r.UpdateAsync(domain))
                .ReturnsAsync(domain);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.UpdateAsync(domain);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [TestMethod]
        public async Task UpdateAsync_DomainNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var domain = CreateValidDomain();
            _domainRepositoryMock.Setup(r => r.ExistsAsync(domain.Id))
                .ReturnsAsync(false);

            // Act
            var result = await _service.UpdateAsync(domain);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Domain with ID '{domain.Id}' not found");
        }

        [TestMethod]
        public async Task UpdateAsync_DuplicateDomainName_ShouldReturnValidationError()
        {
            // Arrange
            var domain = CreateValidDomain();
            var existingDomain = _fixture.Build<Domain>()
                .With(d => d.Id, Guid.NewGuid())
                .Create();

            _domainRepositoryMock.Setup(r => r.ExistsAsync(domain.Id))
                .ReturnsAsync(true);
            _domainRepositoryMock.Setup(r => r.FindByNameAsync(domain.Name))
                .ReturnsAsync(existingDomain);

            // Act
            var result = await _service.UpdateAsync(domain);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain($"Another domain with name '{domain.Name}' already exists");
        }

        [TestMethod]
        public async Task UpdateAsync_SameDomainName_ShouldReturnSuccess()
        {
            // Arrange
            var domain = CreateValidDomain();
            var existingDomain = _fixture.Build<Domain>()
                .With(d => d.Id, domain.Id)
                .Create();

            _domainRepositoryMock.Setup(r => r.ExistsAsync(domain.Id))
                .ReturnsAsync(true);
            _domainRepositoryMock.Setup(r => r.FindByNameAsync(domain.Name))
                .ReturnsAsync(existingDomain);
            _domainRepositoryMock.Setup(r => r.UpdateAsync(domain))
                .ReturnsAsync(domain);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.UpdateAsync(domain);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        #endregion

        #region DeleteAsync Tests (Without Transactions)

        [TestMethod]
        public async Task DeleteAsync_DomainWithoutBooksOrSubdomains_ShouldReturnSuccess()
        {
            // Arrange
            var domainId = Guid.NewGuid();
            var domain = CreateValidDomain();

            _domainRepositoryMock.Setup(r => r.GetByIdAsync(domainId))
                .ReturnsAsync(domain);
            _domainRepositoryMock.Setup(r => r.HasBooksAsync(domainId))
                .ReturnsAsync(false);
            _domainRepositoryMock.Setup(r => r.HasSubdomainsAsync(domainId))
                .ReturnsAsync(false);
            _domainRepositoryMock.Setup(r => r.DeleteAsync(domainId))
                .ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.DeleteAsync(domainId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
            _domainRepositoryMock.Verify(r => r.DeleteAsync(domainId), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task DeleteAsync_DomainNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var domainId = Guid.NewGuid();
            _domainRepositoryMock.Setup(r => r.GetByIdAsync(domainId))
                .ReturnsAsync((Domain)null);

            // Act
            var result = await _service.DeleteAsync(domainId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Domain with ID '{domainId}' not found");
            _domainRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task DeleteAsync_DomainHasBooks_ShouldReturnBusinessRuleError()
        {
            // Arrange
            var domainId = Guid.NewGuid();
            var domain = CreateValidDomain();

            _domainRepositoryMock.Setup(r => r.GetByIdAsync(domainId))
                .ReturnsAsync(domain);
            _domainRepositoryMock.Setup(r => r.HasBooksAsync(domainId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(domainId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Cannot delete a domain that has associated books");
            _domainRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task DeleteAsync_DomainHasSubdomains_ShouldReturnBusinessRuleError()
        {
            // Arrange
            var domainId = Guid.NewGuid();
            var domain = CreateValidDomain();

            _domainRepositoryMock.Setup(r => r.GetByIdAsync(domainId))
                .ReturnsAsync(domain);
            _domainRepositoryMock.Setup(r => r.HasBooksAsync(domainId))
                .ReturnsAsync(false);
            _domainRepositoryMock.Setup(r => r.HasSubdomainsAsync(domainId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(domainId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Cannot delete a domain that has subdomains. Delete subdomains first");
            _domainRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task DeleteAsync_DeleteFails_ShouldReturnFailure()
        {
            // Arrange
            var domainId = Guid.NewGuid();
            var domain = CreateValidDomain();

            _domainRepositoryMock.Setup(r => r.GetByIdAsync(domainId))
                .ReturnsAsync(domain);
            _domainRepositoryMock.Setup(r => r.HasBooksAsync(domainId))
                .ReturnsAsync(false);
            _domainRepositoryMock.Setup(r => r.HasSubdomainsAsync(domainId))
                .ReturnsAsync(false);
            _domainRepositoryMock.Setup(r => r.DeleteAsync(domainId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteAsync(domainId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Failed to delete domain");
            _domainRepositoryMock.Verify(r => r.DeleteAsync(domainId), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        #endregion

        #region ExistsAsync Tests

        [TestMethod]
        public async Task ExistsAsync_DomainExists_ShouldReturnTrue()
        {
            // Arrange
            var domainId = Guid.NewGuid();
            _domainRepositoryMock.Setup(r => r.ExistsAsync(domainId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ExistsAsync(domainId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [TestMethod]
        public async Task ExistsAsync_DomainDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var domainId = Guid.NewGuid();
            _domainRepositoryMock.Setup(r => r.ExistsAsync(domainId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.ExistsAsync(domainId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeFalse();
        }

        #endregion

        #region Exception Handling Tests

        [TestMethod]
        public async Task CreateAsync_RepositoryThrowsException_ShouldReturnFailureWithErrorMessage()
        {
            // Arrange
            var domain = CreateValidDomain();
            _domainRepositoryMock.Setup(r => r.FindByNameAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act
            var result = await _service.CreateAsync(domain);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred");
        }

        [TestMethod]
        public async Task UpdateAsync_RepositoryThrowsException_ShouldReturnFailureWithErrorMessage()
        {
            // Arrange
            var domain = CreateValidDomain();

            _domainRepositoryMock.Setup(r => r.ExistsAsync(domain.Id))
                .ReturnsAsync(true);
            _domainRepositoryMock.Setup(r => r.FindByNameAsync(domain.Name))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act
            var result = await _service.UpdateAsync(domain);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred");
        }

        #endregion

        #region Edge Cases and Boundary Tests

        [TestMethod]
        public async Task CreateAsync_WithMinimumValidNameLength_ShouldSucceed()
        {
            // Arrange
            var domain = CreateValidDomain();
            domain.Name = "AB";

            _domainRepositoryMock.Setup(r => r.FindByNameAsync(domain.Name))
                .ReturnsAsync((Domain)null);
            _domainRepositoryMock.Setup(r => r.AddAsync(domain))
                .ReturnsAsync(domain);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(domain);

            // Assert
            result.Success.Should().BeTrue();
        }

        [TestMethod]
        public async Task CreateAsync_WithMaximumValidNameLength_ShouldSucceed()
        {
            // Arrange
            var domain = CreateValidDomain();
            domain.Name = new string('A', 200);

            _domainRepositoryMock.Setup(r => r.FindByNameAsync(domain.Name))
                .ReturnsAsync((Domain)null);
            _domainRepositoryMock.Setup(r => r.AddAsync(domain))
                .ReturnsAsync(domain);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(domain);

            // Assert
            result.Success.Should().BeTrue();
        }

        [TestMethod]
        public async Task CreateAsync_WithEmptyName_ShouldReturnValidationError()
        {
            // Arrange
            var domain = CreateValidDomain();
            domain.Name = "";

            // Act
            var result = await _service.CreateAsync(domain);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain("Domain name is required");
        }

        [TestMethod]
        public async Task CreateAsync_WithNullName_ShouldReturnValidationError()
        {
            // Arrange
            var domain = CreateValidDomain();
            domain.Name = null;

            // Act
            var result = await _service.CreateAsync(domain);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain("Domain name is required");
        }

        [TestMethod]
        public async Task CreateAsync_WithSelfAsParentDomain_ShouldReturnValidationError()
        {
            // Arrange
            var domain = CreateValidDomain();
            domain.ParentDomain = domain;

            // Act
            var result = await _service.CreateAsync(domain);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().Contain(item => item.Contains("circular reference"));
        }

        #endregion
    }
}
