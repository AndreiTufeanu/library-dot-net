using AutoFixture;
using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLayer.Services;
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
    public class BookTypeServiceTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<BookTypeService>> _loggerMock;
        private readonly BookTypeService _service;

        private readonly Mock<IBookTypeRepository> _bookTypeRepositoryMock;

        public BookTypeServiceTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<BookTypeService>>();

            _bookTypeRepositoryMock = new Mock<IBookTypeRepository>();
            _unitOfWorkMock.Setup(u => u.BookTypes).Returns(_bookTypeRepositoryMock.Object);

            _service = new BookTypeService(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        private BookType CreateValidBookType()
        {
            return _fixture.Build<BookType>()
                .With(bt => bt.Name, "Hardcover")
                .Create();
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WhenUnitOfWorkIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            IUnitOfWork unitOfWork = null;
            ILogger<BookTypeService> logger = Mock.Of<ILogger<BookTypeService>>();

            // Act
            Action act = () => new BookTypeService(unitOfWork, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*unitOfWork*");
        }

        [TestMethod]
        public void Constructor_WhenLoggerIsNull_ShouldThrow()
        {
            // Arrange
            var unitOfWork = Mock.Of<IUnitOfWork>();
            ILogger<BookTypeService> logger = null;

            // Act
            Action act = () => new BookTypeService(unitOfWork, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        #endregion

        #region CreateAsync Tests

        [TestMethod]
        public async Task CreateAsync_ValidBookType_ShouldReturnSuccess()
        {
            // Arrange
            var bookType = CreateValidBookType();
            var addedBookType = _fixture.Build<BookType>()
                .With(bt => bt.Id, bookType.Id)
                .Create();

            _bookTypeRepositoryMock.Setup(r => r.FindByNameAsync(bookType.Name))
                .ReturnsAsync((BookType)null);
            _bookTypeRepositoryMock.Setup(r => r.AddAsync(bookType))
                .ReturnsAsync(addedBookType);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(bookType);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be(addedBookType);
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.ValidationErrors.Should().BeEmpty();
        }

        [TestMethod]
        public async Task CreateAsync_ShouldCallRepositoryMethods()
        {
            // Arrange
            var bookType = CreateValidBookType();

            _bookTypeRepositoryMock.Setup(r => r.FindByNameAsync(bookType.Name))
                .ReturnsAsync((BookType)null);
            _bookTypeRepositoryMock.Setup(r => r.AddAsync(It.IsAny<BookType>()))
                .ReturnsAsync(bookType);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _service.CreateAsync(bookType);

            // Assert
            _bookTypeRepositoryMock.Verify(r => r.FindByNameAsync(bookType.Name), Times.Once);
            _bookTypeRepositoryMock.Verify(r => r.AddAsync(bookType), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task CreateAsync_DuplicateBookType_ShouldReturnValidationError()
        {
            // Arrange
            var bookType = CreateValidBookType();
            var existingBookType = CreateValidBookType();

            _bookTypeRepositoryMock.Setup(r => r.FindByNameAsync(bookType.Name))
                .ReturnsAsync(existingBookType);

            // Act
            var result = await _service.CreateAsync(bookType);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain($"A book type with the name '{bookType.Name}' already exists");
        }

        [TestMethod]
        public async Task CreateAsync_ValidationFailure_ShouldReturnValidationErrors()
        {
            // Arrange
            var bookType = CreateValidBookType();
            bookType.Name = "A";

            // Act
            var result = await _service.CreateAsync(bookType);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain("Book type name must be between");
        }

        [TestMethod]
        public async Task CreateAsync_InvalidNameCharacters_ShouldReturnValidationError()
        {
            // Arrange
            var bookType = CreateValidBookType();
            bookType.Name = "Hardcover123";

            // Act
            var result = await _service.CreateAsync(bookType);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain("Book type name can only contain letters and spaces");
        }

        #endregion

        #region GetByIdAsync Tests

        [TestMethod]
        public async Task GetByIdAsync_BookTypeExists_ShouldReturnBookType()
        {
            // Arrange
            var bookTypeId = Guid.NewGuid();
            var bookType = _fixture.Build<BookType>()
                .With(bt => bt.Id, bookTypeId)
                .Create();

            _bookTypeRepositoryMock.Setup(r => r.GetByIdAsync(bookTypeId))
                .ReturnsAsync(bookType);

            // Act
            var result = await _service.GetByIdAsync(bookTypeId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be(bookType);
        }

        [TestMethod]
        public async Task GetByIdAsync_BookTypeNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var bookTypeId = Guid.NewGuid();
            _bookTypeRepositoryMock.Setup(r => r.GetByIdAsync(bookTypeId))
                .ReturnsAsync((BookType)null);

            // Act
            var result = await _service.GetByIdAsync(bookTypeId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"BookType with ID '{bookTypeId}' not found");
        }

        #endregion

        #region GetAllAsync Tests

        [TestMethod]
        public async Task GetAllAsync_BookTypesExist_ShouldReturnAllBookTypes()
        {
            // Arrange
            var bookTypes = _fixture.CreateMany<BookType>(3).ToList();
            _bookTypeRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(bookTypes);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(bookTypes);
        }

        [TestMethod]
        public async Task GetAllAsync_NoBookTypes_ShouldReturnEmptyList()
        {
            // Arrange
            _bookTypeRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<BookType>());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region UpdateAsync Tests

        [TestMethod]
        public async Task UpdateAsync_ValidBookType_ShouldReturnSuccess()
        {
            // Arrange
            var bookType = CreateValidBookType();

            _bookTypeRepositoryMock.Setup(r => r.ExistsAsync(bookType.Id))
                .ReturnsAsync(true);
            _bookTypeRepositoryMock.Setup(r => r.FindByNameAsync(bookType.Name))
                .ReturnsAsync((BookType)null);
            _bookTypeRepositoryMock.Setup(r => r.UpdateAsync(bookType))
                .ReturnsAsync(bookType);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.UpdateAsync(bookType);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [TestMethod]
        public async Task UpdateAsync_BookTypeNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var bookType = CreateValidBookType();
            _bookTypeRepositoryMock.Setup(r => r.ExistsAsync(bookType.Id))
                .ReturnsAsync(false);

            // Act
            var result = await _service.UpdateAsync(bookType);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"BookType with ID '{bookType.Id}' not found");
        }

        [TestMethod]
        public async Task UpdateAsync_DuplicateBookTypeName_ShouldReturnValidationError()
        {
            // Arrange
            var bookType = CreateValidBookType();
            var existingBookType = _fixture.Build<BookType>()
                .With(bt => bt.Id, Guid.NewGuid())
                .Create();

            _bookTypeRepositoryMock.Setup(r => r.ExistsAsync(bookType.Id))
                .ReturnsAsync(true);
            _bookTypeRepositoryMock.Setup(r => r.FindByNameAsync(bookType.Name))
                .ReturnsAsync(existingBookType);

            // Act
            var result = await _service.UpdateAsync(bookType);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain($"Another book type with the name '{bookType.Name}' already exists");
        }

        [TestMethod]
        public async Task UpdateAsync_SameBookTypeName_ShouldReturnSuccess()
        {
            // Arrange
            var bookType = CreateValidBookType();
            var existingBookType = _fixture.Build<BookType>()
                .With(bt => bt.Id, bookType.Id)
                .Create();

            _bookTypeRepositoryMock.Setup(r => r.ExistsAsync(bookType.Id))
                .ReturnsAsync(true);
            _bookTypeRepositoryMock.Setup(r => r.FindByNameAsync(bookType.Name))
                .ReturnsAsync(existingBookType);
            _bookTypeRepositoryMock.Setup(r => r.UpdateAsync(bookType))
                .ReturnsAsync(bookType);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.UpdateAsync(bookType);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        #endregion

        #region DeleteAsync Tests

        [TestMethod]
        public async Task DeleteAsync_BookTypeWithoutEditions_ShouldReturnSuccess()
        {
            // Arrange
            var bookTypeId = Guid.NewGuid();
            var bookType = CreateValidBookType();

            _bookTypeRepositoryMock.Setup(r => r.GetByIdAsync(bookTypeId))
                .ReturnsAsync(bookType);
            _bookTypeRepositoryMock.Setup(r => r.HasEditionsAsync(bookTypeId))
                .ReturnsAsync(false);
            _bookTypeRepositoryMock.Setup(r => r.DeleteAsync(bookTypeId))
                .ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.DeleteAsync(bookTypeId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
            _bookTypeRepositoryMock.Verify(r => r.DeleteAsync(bookTypeId), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task DeleteAsync_BookTypeNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var bookTypeId = Guid.NewGuid();
            _bookTypeRepositoryMock.Setup(r => r.GetByIdAsync(bookTypeId))
                .ReturnsAsync((BookType)null);

            // Act
            var result = await _service.DeleteAsync(bookTypeId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"BookType with ID '{bookTypeId}' not found");
            _bookTypeRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task DeleteAsync_BookTypeHasEditions_ShouldReturnBusinessRuleError()
        {
            // Arrange
            var bookTypeId = Guid.NewGuid();
            var bookType = CreateValidBookType();

            _bookTypeRepositoryMock.Setup(r => r.GetByIdAsync(bookTypeId))
                .ReturnsAsync(bookType);
            _bookTypeRepositoryMock.Setup(r => r.HasEditionsAsync(bookTypeId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(bookTypeId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Cannot delete a book type that has associated editions");
            _bookTypeRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task DeleteAsync_DeleteFails_ShouldReturnFailure()
        {
            // Arrange
            var bookTypeId = Guid.NewGuid();
            var bookType = CreateValidBookType();

            _bookTypeRepositoryMock.Setup(r => r.GetByIdAsync(bookTypeId))
                .ReturnsAsync(bookType);
            _bookTypeRepositoryMock.Setup(r => r.HasEditionsAsync(bookTypeId))
                .ReturnsAsync(false);
            _bookTypeRepositoryMock.Setup(r => r.DeleteAsync(bookTypeId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteAsync(bookTypeId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Failed to delete book type");
            _bookTypeRepositoryMock.Verify(r => r.DeleteAsync(bookTypeId), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task DeleteAsync_WhenBusinessRuleExceptionThrown_ShouldReturnFailureWithBusinessRuleMessage()
        {
            // Arrange
            var bookTypeId = Guid.NewGuid();
            var bookType = CreateValidBookType();

            _bookTypeRepositoryMock.Setup(r => r.GetByIdAsync(bookTypeId))
                .ReturnsAsync(bookType);
            _bookTypeRepositoryMock.Setup(r => r.HasEditionsAsync(bookTypeId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(bookTypeId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Cannot delete a book type that has associated editions");
        }

        [TestMethod]
        public async Task DeleteAsync_WhenNotFoundExceptionThrown_ShouldReturnFailureWithNotFoundMessage()
        {
            // Arrange
            var bookTypeId = Guid.NewGuid();
            _bookTypeRepositoryMock.Setup(r => r.GetByIdAsync(bookTypeId))
                .ReturnsAsync((BookType)null);

            // Act
            var result = await _service.DeleteAsync(bookTypeId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"BookType with ID '{bookTypeId}' not found");
        }


        #endregion

        #region ExistsAsync Tests

        [TestMethod]
        public async Task ExistsAsync_BookTypeExists_ShouldReturnTrue()
        {
            // Arrange
            var bookTypeId = Guid.NewGuid();
            _bookTypeRepositoryMock.Setup(r => r.ExistsAsync(bookTypeId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ExistsAsync(bookTypeId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [TestMethod]
        public async Task ExistsAsync_BookTypeDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var bookTypeId = Guid.NewGuid();
            _bookTypeRepositoryMock.Setup(r => r.ExistsAsync(bookTypeId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.ExistsAsync(bookTypeId);

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
            var bookType = CreateValidBookType();
            _bookTypeRepositoryMock.Setup(r => r.FindByNameAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act
            var result = await _service.CreateAsync(bookType);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred");
        }

        [TestMethod]
        public async Task UpdateAsync_RepositoryThrowsException_ShouldReturnFailureWithErrorMessage()
        {
            // Arrange
            var bookType = CreateValidBookType();

            _bookTypeRepositoryMock.Setup(r => r.ExistsAsync(bookType.Id))
                .ReturnsAsync(true);
            _bookTypeRepositoryMock.Setup(r => r.FindByNameAsync(bookType.Name))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act
            var result = await _service.UpdateAsync(bookType);

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
            var bookType = CreateValidBookType();
            bookType.Name = "AB";

            _bookTypeRepositoryMock.Setup(r => r.FindByNameAsync(bookType.Name))
                .ReturnsAsync((BookType)null);
            _bookTypeRepositoryMock.Setup(r => r.AddAsync(bookType))
                .ReturnsAsync(bookType);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(bookType);

            // Assert
            result.Success.Should().BeTrue();
        }

        [TestMethod]
        public async Task CreateAsync_WithMaximumValidNameLength_ShouldSucceed()
        {
            // Arrange
            var bookType = CreateValidBookType();
            bookType.Name = new string('A', 50);

            _bookTypeRepositoryMock.Setup(r => r.FindByNameAsync(bookType.Name))
                .ReturnsAsync((BookType)null);
            _bookTypeRepositoryMock.Setup(r => r.AddAsync(bookType))
                .ReturnsAsync(bookType);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(bookType);

            // Assert
            result.Success.Should().BeTrue();
        }

        [TestMethod]
        public async Task CreateAsync_WithNameContainingSpaces_ShouldSucceed()
        {
            // Arrange
            var bookType = CreateValidBookType();
            bookType.Name = "Hard Cover";

            _bookTypeRepositoryMock.Setup(r => r.FindByNameAsync(bookType.Name))
                .ReturnsAsync((BookType)null);
            _bookTypeRepositoryMock.Setup(r => r.AddAsync(bookType))
                .ReturnsAsync(bookType);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(bookType);

            // Assert
            result.Success.Should().BeTrue();
        }

        [TestMethod]
        public async Task CreateAsync_WithEmptyName_ShouldReturnValidationError()
        {
            // Arrange
            var bookType = CreateValidBookType();
            bookType.Name = "";

            // Act
            var result = await _service.CreateAsync(bookType);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain("Book type name is required");
        }

        [TestMethod]
        public async Task CreateAsync_WithNullName_ShouldReturnValidationError()
        {
            // Arrange
            var bookType = CreateValidBookType();
            bookType.Name = null;

            // Act
            var result = await _service.CreateAsync(bookType);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain("Book type name is required");
        }

        #endregion
    }
}
