using AutoFixture;
using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLayer.Exceptions;
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
    public class BookCopyServiceTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<BookCopyService>> _loggerMock;
        private readonly BookCopyService _service;

        private readonly Mock<IBookCopyRepository> _bookCopyRepositoryMock;
        private readonly Mock<IEditionRepository> _editionRepositoryMock;

        public BookCopyServiceTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<BookCopyService>>();

            // Setup repository mocks
            _bookCopyRepositoryMock = new Mock<IBookCopyRepository>();
            _editionRepositoryMock = new Mock<IEditionRepository>();

            // Setup unit of work to return repository mocks
            _unitOfWorkMock.Setup(u => u.BookCopies).Returns(_bookCopyRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.Editions).Returns(_editionRepositoryMock.Object);

            _service = new BookCopyService(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        private BookCopy CreateValidBookCopy(bool isLectureRoomOnly = false)
        {
            var edition = _fixture.Build<Edition>()
                .With(e => e.Id, Guid.NewGuid())
                .Create();

            return _fixture.Build<BookCopy>()
                .FromFactory(() => new BookCopy(isLectureRoomOnly: isLectureRoomOnly))
                .With(bc => bc.Edition, edition)
                .Create();
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WhenUnitOfWorkIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            IUnitOfWork unitOfWork = null;
            IValidator<BookCopy> validator = Mock.Of<IValidator<BookCopy>>();
            ILogger<BookCopyService> logger = Mock.Of<ILogger<BookCopyService>>();

            // Act
            Action act = () => new BookCopyService(unitOfWork, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*unitOfWork*");
        }

        [TestMethod]
        public void Constructor_WhenLoggerIsNull_ShouldNotThrow()
        {
            // Arrange
            var unitOfWork = Mock.Of<IUnitOfWork>();
            var validator = Mock.Of<IValidator<BookCopy>>();
            ILogger<BookCopyService> logger = null;

            // Act
            Action act = () => new BookCopyService(unitOfWork, logger);

            // Assert
            act.Should().NotThrow();
        }

        #endregion

        #region CreateAsync Tests

        [TestMethod]
        public async Task CreateAsync_EditionDoesNotExist_ShouldThrowNotFoundException()
        {
            // Arrange
            var bookCopy = CreateValidBookCopy();

            _editionRepositoryMock.Setup(r => r.ExistsAsync(bookCopy.Edition.Id))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(bookCopy);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Edition");
        }

        [TestMethod]
        public async Task CreateAsync_ValidBookCopy_ShouldReturnSuccess()
        {
            // Arrange
            var bookCopy = CreateValidBookCopy();
            var addedBookCopy = _fixture.Build<BookCopy>()
                .With(bc => bc.Id, bookCopy.Id)
                .Create();

            _editionRepositoryMock.Setup(r => r.ExistsAsync(bookCopy.Edition.Id))
                .ReturnsAsync(true);
            _bookCopyRepositoryMock.Setup(r => r.AddAsync(bookCopy))
                .ReturnsAsync(addedBookCopy);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(bookCopy);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be(addedBookCopy);
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.ValidationErrors.Should().BeEmpty();
        }

        [TestMethod]
        public async Task CreateAsync_ShouldCallRepositoryMethods()
        {
            // Arrange
            var bookCopy = CreateValidBookCopy();

            _editionRepositoryMock.Setup(r => r.ExistsAsync(bookCopy.Edition.Id))
                .ReturnsAsync(true);
            _bookCopyRepositoryMock.Setup(r => r.AddAsync(It.IsAny<BookCopy>()))
                .ReturnsAsync(bookCopy);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _service.CreateAsync(bookCopy);

            // Assert
            _editionRepositoryMock.Verify(r => r.ExistsAsync(bookCopy.Edition.Id), Times.Once);
            _bookCopyRepositoryMock.Verify(r => r.AddAsync(bookCopy), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task CreateAsync_EditionNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var bookCopy = CreateValidBookCopy();
            _editionRepositoryMock.Setup(r => r.ExistsAsync(bookCopy.Edition.Id))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(bookCopy);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Edition with ID '{bookCopy.Edition.Id}' not found");
        }

        [TestMethod]
        public async Task CreateAsync_ValidationFailure_ShouldReturnValidationErrors()
        {
            // Arrange
            var bookCopy = CreateValidBookCopy();
            bookCopy.Edition = null;

            // Act
            var result = await _service.CreateAsync(bookCopy);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().Contain(item => item.Contains("Edition is required"));
        }

        #endregion

        #region GetByIdAsync Tests

        [TestMethod]
        public async Task GetByIdAsync_BookCopyExists_ShouldReturnBookCopy()
        {
            // Arrange
            var bookCopyId = Guid.NewGuid();
            var bookCopy = _fixture.Build<BookCopy>()
                .With(bc => bc.Id, bookCopyId)
                .Create();

            _bookCopyRepositoryMock.Setup(r => r.GetByIdAsync(bookCopyId))
                .ReturnsAsync(bookCopy);

            // Act
            var result = await _service.GetByIdAsync(bookCopyId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be(bookCopy);
        }

        [TestMethod]
        public async Task GetByIdAsync_BookCopyNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var bookCopyId = Guid.NewGuid();
            _bookCopyRepositoryMock.Setup(r => r.GetByIdAsync(bookCopyId))
                .ReturnsAsync((BookCopy)null);

            // Act
            var result = await _service.GetByIdAsync(bookCopyId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"BookCopy with ID '{bookCopyId}' not found");
        }

        #endregion

        #region GetAllAsync Tests

        [TestMethod]
        public async Task GetAllAsync_BookCopiesExist_ShouldReturnAllBookCopies()
        {
            // Arrange
            var bookCopies = _fixture.CreateMany<BookCopy>(3).ToList();
            _bookCopyRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(bookCopies);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(bookCopies);
        }

        [TestMethod]
        public async Task GetAllAsync_NoBookCopies_ShouldReturnEmptyList()
        {
            // Arrange
            _bookCopyRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<BookCopy>());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region UpdateAsync Tests

        [TestMethod]
        public async Task UpdateAsync_ValidBookCopy_ShouldReturnSuccess()
        {
            // Arrange
            var bookCopy = CreateValidBookCopy();

            _bookCopyRepositoryMock.Setup(r => r.ExistsAsync(bookCopy.Id))
                .ReturnsAsync(true);
            _bookCopyRepositoryMock.Setup(r => r.UpdateAsync(bookCopy))
                .ReturnsAsync(bookCopy);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.UpdateAsync(bookCopy);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [TestMethod]
        public async Task UpdateAsync_BookCopyNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var bookCopy = CreateValidBookCopy();
            _bookCopyRepositoryMock.Setup(r => r.ExistsAsync(bookCopy.Id))
                .ReturnsAsync(false);

            // Act
            var result = await _service.UpdateAsync(bookCopy);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"BookCopy with ID '{bookCopy.Id}' not found");
        }

        #endregion

        #region DeleteAsync Tests (Without Transactions)

        [TestMethod]
        public async Task DeleteAsync_BookCopyNotCurrentlyBorrowed_ShouldReturnSuccess()
        {
            // Arrange
            var bookCopyId = Guid.NewGuid();
            var bookCopy = CreateValidBookCopy();

            _bookCopyRepositoryMock.Setup(r => r.GetByIdAsync(bookCopyId))
                .ReturnsAsync(bookCopy);
            _bookCopyRepositoryMock.Setup(r => r.IsCurrentlyBorrowedAsync(bookCopyId))
                .ReturnsAsync(false);
            _bookCopyRepositoryMock.Setup(r => r.DeleteAsync(bookCopyId))
                .ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.DeleteAsync(bookCopyId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
            _bookCopyRepositoryMock.Verify(r => r.DeleteAsync(bookCopyId), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task DeleteAsync_BookCopyNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var bookCopyId = Guid.NewGuid();
            _bookCopyRepositoryMock.Setup(r => r.GetByIdAsync(bookCopyId))
                .ReturnsAsync((BookCopy)null);

            // Act
            var result = await _service.DeleteAsync(bookCopyId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"BookCopy with ID '{bookCopyId}' not found");
            _bookCopyRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task DeleteAsync_BookCopyCurrentlyBorrowed_ShouldReturnBusinessRuleError()
        {
            // Arrange
            var bookCopyId = Guid.NewGuid();
            var bookCopy = CreateValidBookCopy();

            _bookCopyRepositoryMock.Setup(r => r.GetByIdAsync(bookCopyId))
                .ReturnsAsync(bookCopy);
            _bookCopyRepositoryMock.Setup(r => r.IsCurrentlyBorrowedAsync(bookCopyId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(bookCopyId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Cannot delete a book copy that is currently borrowed");
            _bookCopyRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task DeleteAsync_DeleteFails_ShouldReturnFailure()
        {
            // Arrange
            var bookCopyId = Guid.NewGuid();
            var bookCopy = CreateValidBookCopy();

            _bookCopyRepositoryMock.Setup(r => r.GetByIdAsync(bookCopyId))
                .ReturnsAsync(bookCopy);
            _bookCopyRepositoryMock.Setup(r => r.IsCurrentlyBorrowedAsync(bookCopyId))
                .ReturnsAsync(false);
            _bookCopyRepositoryMock.Setup(r => r.DeleteAsync(bookCopyId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteAsync(bookCopyId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Failed to delete book copy");
            _bookCopyRepositoryMock.Verify(r => r.DeleteAsync(bookCopyId), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        #endregion

        #region ExistsAsync Tests

        [TestMethod]
        public async Task ExistsAsync_BookCopyExists_ShouldReturnTrue()
        {
            // Arrange
            var bookCopyId = Guid.NewGuid();
            _bookCopyRepositoryMock.Setup(r => r.ExistsAsync(bookCopyId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ExistsAsync(bookCopyId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [TestMethod]
        public async Task ExistsAsync_BookCopyDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var bookCopyId = Guid.NewGuid();
            _bookCopyRepositoryMock.Setup(r => r.ExistsAsync(bookCopyId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.ExistsAsync(bookCopyId);

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
            var bookCopy = CreateValidBookCopy();

            _editionRepositoryMock.Setup(r => r.ExistsAsync(bookCopy.Edition.Id))
                .ReturnsAsync(true);
            _bookCopyRepositoryMock.Setup(r => r.AddAsync(It.IsAny<BookCopy>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act
            var result = await _service.CreateAsync(bookCopy);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred");
        }

        [TestMethod]
        public async Task UpdateAsync_RepositoryThrowsException_ShouldReturnFailureWithErrorMessage()
        {
            // Arrange
            var bookCopy = CreateValidBookCopy();

            _bookCopyRepositoryMock.Setup(r => r.ExistsAsync(bookCopy.Id))
                .ReturnsAsync(true);
            _bookCopyRepositoryMock.Setup(r => r.UpdateAsync(bookCopy))
                .ThrowsAsync(new InvalidOperationException("Database constraint violation"));

            // Act
            var result = await _service.UpdateAsync(bookCopy);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred");
        }

        #endregion

        #region Edge Cases and Boundary Tests

        [TestMethod]
        public async Task CreateAsync_WithLectureRoomOnlyTrue_ShouldSucceed()
        {
            // Arrange
            var bookCopy = CreateValidBookCopy(true);

            _editionRepositoryMock.Setup(r => r.ExistsAsync(bookCopy.Edition.Id))
                .ReturnsAsync(true);
            _bookCopyRepositoryMock.Setup(r => r.AddAsync(bookCopy))
                .ReturnsAsync(bookCopy);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(bookCopy);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.IsLectureRoomOnly.Should().BeTrue();
        }

        [TestMethod]
        public async Task CreateAsync_WithLectureRoomOnlyFalse_ShouldSucceed()
        {
            // Arrange
            var bookCopy = CreateValidBookCopy(false);

            _editionRepositoryMock.Setup(r => r.ExistsAsync(bookCopy.Edition.Id))
                .ReturnsAsync(true);
            _bookCopyRepositoryMock.Setup(r => r.AddAsync(bookCopy))
                .ReturnsAsync(bookCopy);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(bookCopy);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.IsLectureRoomOnly.Should().BeFalse();
        }

        [TestMethod]
        public async Task CreateAsync_WithNullEdition_ShouldReturnValidationError()
        {
            // Arrange
            var bookCopy = CreateValidBookCopy();
            bookCopy.Edition = null;

            // Act
            var result = await _service.CreateAsync(bookCopy);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().Contain(item => item.Contains("Edition is required"));
        }

        #endregion
    }
}
