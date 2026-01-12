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
    public class ReaderServiceTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<ReaderService>> _loggerMock;
        private readonly IValidator<Reader> _validator;
        private readonly ReaderService _service;

        private readonly Mock<IReaderRepository> _readerRepositoryMock;
        private readonly Mock<IBorrowingRepository> _borrowingRepositoryMock;

        public ReaderServiceTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<ReaderService>>();

            // Setup repository mocks
            _readerRepositoryMock = new Mock<IReaderRepository>();
            _borrowingRepositoryMock = new Mock<IBorrowingRepository>();

            // Setup unit of work to return repository mocks
            _unitOfWorkMock.Setup(u => u.Readers).Returns(_readerRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.Borrowings).Returns(_borrowingRepositoryMock.Object);

            _validator = new ReaderValidator();
            _service = new ReaderService(_unitOfWorkMock.Object, _validator, _loggerMock.Object);
        }

        private Reader CreateValidReader()
        {
            return _fixture.Build<Reader>()
                .With(r => r.FirstName, "John")
                .With(r => r.LastName, "Doe")
                .With(r => r.Address, "123 Main St, City, Country")
                .With(r => r.DateOfBirth, DateTime.Now.AddYears(-20))
                .With(r => r.PhoneNumber, "+1234567890")
                .With(r => r.Email, "john.doe@example.com")
                .Create();
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WhenUnitOfWorkIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            IUnitOfWork unitOfWork = null;
            IValidator<Reader> validator = Mock.Of<IValidator<Reader>>();
            ILogger<ReaderService> logger = Mock.Of<ILogger<ReaderService>>();

            // Act
            Action act = () => new ReaderService(unitOfWork, validator, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*unitOfWork*");
        }

        [TestMethod]
        public void Constructor_WhenValidatorIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var unitOfWork = Mock.Of<IUnitOfWork>();
            IValidator<Reader> validator = null;
            var logger = Mock.Of<ILogger<ReaderService>>();

            // Act
            Action act = () => new ReaderService(unitOfWork, validator, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*validator*");
        }

        [TestMethod]
        public void Constructor_WhenLoggerIsNull_ShouldThrow()
        {
            // Arrange
            var unitOfWork = Mock.Of<IUnitOfWork>();
            var validator = Mock.Of<IValidator<Reader>>();
            ILogger<ReaderService> logger = null;

            // Act
            Action act = () => new ReaderService(unitOfWork, validator, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        #endregion

        #region CreateAsync Tests

        [TestMethod]
        public async Task CreateAsync_ValidReader_ShouldReturnSuccess()
        {
            // Arrange
            var reader = CreateValidReader();
            var addedReader = _fixture.Build<Reader>()
                .With(r => r.Id, reader.Id)
                .Create();

            _readerRepositoryMock.Setup(r => r.FindByEmailAsync(reader.Email))
                .ReturnsAsync((Reader)null);
            _readerRepositoryMock.Setup(r => r.FindByPhoneAsync(reader.PhoneNumber))
                .ReturnsAsync((Reader)null);
            _readerRepositoryMock.Setup(r => r.AddAsync(reader))
                .ReturnsAsync(addedReader);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(reader);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be(addedReader);
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.ValidationErrors.Should().BeEmpty();
        }

        [TestMethod]
        public async Task CreateAsync_DuplicateEmail_ShouldReturnValidationError()
        {
            // Arrange
            var reader = CreateValidReader();
            var existingReader = CreateValidReader();

            _readerRepositoryMock.Setup(r => r.FindByEmailAsync(reader.Email))
                .ReturnsAsync(existingReader);

            // Act
            var result = await _service.CreateAsync(reader);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain($"Reader with email '{reader.Email}' already exists");
        }

        [TestMethod]
        public async Task CreateAsync_DuplicatePhone_ShouldReturnValidationError()
        {
            // Arrange
            var reader = CreateValidReader();
            var existingReader = CreateValidReader();

            _readerRepositoryMock.Setup(r => r.FindByEmailAsync(reader.Email))
                .ReturnsAsync((Reader)null);
            _readerRepositoryMock.Setup(r => r.FindByPhoneAsync(reader.PhoneNumber))
                .ReturnsAsync(existingReader);

            // Act
            var result = await _service.CreateAsync(reader);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain($"Reader with phone number '{reader.PhoneNumber}' already exists");
        }

        #endregion

        #region GetByIdAsync Tests

        [TestMethod]
        public async Task GetByIdAsync_ReaderExists_ShouldReturnReader()
        {
            // Arrange
            var readerId = Guid.NewGuid();
            var reader = _fixture.Build<Reader>()
                .With(r => r.Id, readerId)
                .Create();

            _readerRepositoryMock.Setup(r => r.GetByIdAsync(readerId))
                .ReturnsAsync(reader);

            // Act
            var result = await _service.GetByIdAsync(readerId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be(reader);
        }

        [TestMethod]
        public async Task GetByIdAsync_ReaderNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var readerId = Guid.NewGuid();
            _readerRepositoryMock.Setup(r => r.GetByIdAsync(readerId))
                .ReturnsAsync((Reader)null);

            // Act
            var result = await _service.GetByIdAsync(readerId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Reader with ID '{readerId}' not found");
        }

        #endregion

        #region GetAllAsync Tests

        [TestMethod]
        public async Task GetAllAsync_ReadersExist_ShouldReturnAllReaders()
        {
            // Arrange
            var readers = _fixture.CreateMany<Reader>(3).ToList();
            _readerRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(readers);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(readers);
        }

        [TestMethod]
        public async Task GetAllAsync_NoReaders_ShouldReturnEmptyList()
        {
            // Arrange
            _readerRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Reader>());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region UpdateAsync Tests

        [TestMethod]
        public async Task UpdateAsync_ValidReader_ShouldReturnSuccess()
        {
            // Arrange
            var reader = CreateValidReader();

            _readerRepositoryMock.Setup(r => r.ExistsAsync(reader.Id))
                .ReturnsAsync(true);
            _readerRepositoryMock.Setup(r => r.FindByEmailAsync(reader.Email))
                .ReturnsAsync((Reader)null);
            _readerRepositoryMock.Setup(r => r.FindByPhoneAsync(reader.PhoneNumber))
                .ReturnsAsync((Reader)null);
            _readerRepositoryMock.Setup(r => r.UpdateAsync(reader))
                .ReturnsAsync(reader);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.UpdateAsync(reader);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [TestMethod]
        public async Task UpdateAsync_ReaderNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var reader = CreateValidReader();
            _readerRepositoryMock.Setup(r => r.ExistsAsync(reader.Id))
                .ReturnsAsync(false);

            // Act
            var result = await _service.UpdateAsync(reader);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Reader with ID '{reader.Id}' not found");
        }

        [TestMethod]
        public async Task UpdateAsync_DuplicateEmail_ShouldReturnValidationError()
        {
            // Arrange
            var reader = CreateValidReader();
            var existingReader = _fixture.Build<Reader>()
                .With(r => r.Id, Guid.NewGuid())
                .Create();

            _readerRepositoryMock.Setup(r => r.ExistsAsync(reader.Id))
                .ReturnsAsync(true);
            _readerRepositoryMock.Setup(r => r.FindByEmailAsync(reader.Email))
                .ReturnsAsync(existingReader);

            // Act
            var result = await _service.UpdateAsync(reader);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain($"Another reader with email '{reader.Email}' already exists");
        }

        [TestMethod]
        public async Task UpdateAsync_DuplicatePhone_ShouldReturnValidationError()
        {
            // Arrange
            var reader = CreateValidReader();
            var existingReader = _fixture.Build<Reader>()
                .With(r => r.Id, Guid.NewGuid())
                .Create();

            _readerRepositoryMock.Setup(r => r.ExistsAsync(reader.Id))
                .ReturnsAsync(true);
            _readerRepositoryMock.Setup(r => r.FindByEmailAsync(reader.Email))
                .ReturnsAsync((Reader)null);
            _readerRepositoryMock.Setup(r => r.FindByPhoneAsync(reader.PhoneNumber))
                .ReturnsAsync(existingReader);

            // Act
            var result = await _service.UpdateAsync(reader);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain($"Another reader with phone number '{reader.PhoneNumber}' already exists");
        }

        #endregion

        #region DeleteAsync Tests

        [TestMethod]
        public async Task DeleteAsync_ReaderWithoutActiveBorrowings_ShouldReturnSuccess()
        {
            // Arrange
            var readerId = Guid.NewGuid();
            var reader = CreateValidReader();

            _readerRepositoryMock.Setup(r => r.GetByIdAsync(readerId))
                .ReturnsAsync(reader);
            _readerRepositoryMock.Setup(r => r.HasActiveBorrowingsAsync(readerId))
                .ReturnsAsync(false);
            _readerRepositoryMock.Setup(r => r.DeleteAsync(readerId))
                .ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.DeleteAsync(readerId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
            _readerRepositoryMock.Verify(r => r.DeleteAsync(readerId), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task DeleteAsync_ReaderNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var readerId = Guid.NewGuid();
            _readerRepositoryMock.Setup(r => r.GetByIdAsync(readerId))
                .ReturnsAsync((Reader)null);

            // Act
            var result = await _service.DeleteAsync(readerId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Reader with ID '{readerId}' not found");
            _readerRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task DeleteAsync_ReaderHasActiveBorrowings_ShouldReturnBusinessRuleError()
        {
            // Arrange
            var readerId = Guid.NewGuid();
            var reader = CreateValidReader();

            _readerRepositoryMock.Setup(r => r.GetByIdAsync(readerId))
                .ReturnsAsync(reader);
            _readerRepositoryMock.Setup(r => r.HasActiveBorrowingsAsync(readerId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(readerId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Cannot delete a reader that has active borrowings");
            _readerRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task DeleteAsync_DeleteFails_ShouldReturnFailure()
        {
            // Arrange
            var readerId = Guid.NewGuid();
            var reader = CreateValidReader();

            _readerRepositoryMock.Setup(r => r.GetByIdAsync(readerId))
                .ReturnsAsync(reader);
            _readerRepositoryMock.Setup(r => r.HasActiveBorrowingsAsync(readerId))
                .ReturnsAsync(false);
            _readerRepositoryMock.Setup(r => r.DeleteAsync(readerId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteAsync(readerId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Failed to delete reader");
            _readerRepositoryMock.Verify(r => r.DeleteAsync(readerId), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        #endregion

        #region ExistsAsync Tests

        [TestMethod]
        public async Task ExistsAsync_ReaderExists_ShouldReturnTrue()
        {
            // Arrange
            var readerId = Guid.NewGuid();
            _readerRepositoryMock.Setup(r => r.ExistsAsync(readerId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ExistsAsync(readerId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [TestMethod]
        public async Task ExistsAsync_ReaderDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var readerId = Guid.NewGuid();
            _readerRepositoryMock.Setup(r => r.ExistsAsync(readerId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.ExistsAsync(readerId);

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
            var reader = CreateValidReader();

            _readerRepositoryMock.Setup(r => r.FindByEmailAsync(reader.Email))
                .ReturnsAsync((Reader)null);
            _readerRepositoryMock.Setup(r => r.FindByPhoneAsync(reader.PhoneNumber))
                .ReturnsAsync((Reader)null);
            _readerRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Reader>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act
            var result = await _service.CreateAsync(reader);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred");
        }

        [TestMethod]
        public async Task UpdateAsync_RepositoryThrowsException_ShouldReturnFailureWithErrorMessage()
        {
            // Arrange
            var reader = CreateValidReader();

            _readerRepositoryMock.Setup(r => r.ExistsAsync(reader.Id))
                .ReturnsAsync(true);
            _readerRepositoryMock.Setup(r => r.FindByEmailAsync(reader.Email))
                .ReturnsAsync((Reader)null);
            _readerRepositoryMock.Setup(r => r.FindByPhoneAsync(reader.PhoneNumber))
                .ReturnsAsync((Reader)null);
            _readerRepositoryMock.Setup(r => r.UpdateAsync(reader))
                .ThrowsAsync(new InvalidOperationException("Database constraint violation"));

            // Act
            var result = await _service.UpdateAsync(reader);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred");
        }

        [TestMethod]
        public async Task DeleteAsync_RepositoryThrowsException_ShouldReturnFailureWithErrorMessage()
        {
            // Arrange
            var readerId = Guid.NewGuid();
            var reader = CreateValidReader();

            _readerRepositoryMock.Setup(r => r.GetByIdAsync(readerId))
                .ReturnsAsync(reader);
            _readerRepositoryMock.Setup(r => r.HasActiveBorrowingsAsync(readerId))
                .ReturnsAsync(false);
            _readerRepositoryMock.Setup(r => r.DeleteAsync(readerId))
                .ThrowsAsync(new InvalidOperationException("Database constraint violation"));

            // Act
            var result = await _service.DeleteAsync(readerId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred");
        }

        #endregion
    }
}
