using AutoFixture;
using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLayer.Exceptions;
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
    public class EditionServiceTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<EditionService>> _loggerMock;
        private readonly IValidator<Edition> _validator;
        private readonly EditionService _service;

        private readonly Mock<IEditionRepository> _editionRepositoryMock;
        private readonly Mock<IBookRepository> _bookRepositoryMock;
        private readonly Mock<IBookTypeRepository> _bookTypeRepositoryMock;

        public EditionServiceTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<EditionService>>();

            // Setup repository mocks
            _editionRepositoryMock = new Mock<IEditionRepository>();
            _bookRepositoryMock = new Mock<IBookRepository>();
            _bookTypeRepositoryMock = new Mock<IBookTypeRepository>();

            // Setup unit of work to return repository mocks
            _unitOfWorkMock.Setup(u => u.Editions).Returns(_editionRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.Books).Returns(_bookRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.BookTypes).Returns(_bookTypeRepositoryMock.Object);

            _validator = new EditionValidator();
            _service = new EditionService(_unitOfWorkMock.Object, _validator, _loggerMock.Object);
        }

        private Edition CreateValidEdition()
        {
            var book = _fixture.Build<Book>()
                .With(b => b.Id, Guid.NewGuid())
                .Create();

            var bookType = _fixture.Build<BookType>()
                .With(bt => bt.Id, Guid.NewGuid())
                .Create();

            return _fixture.Build<Edition>()
                .With(e => e.NumberOfPages, 300)
                .With(e => e.PublicationDate, DateTime.Now.AddMonths(-6))
                .With(e => e.Book, book)
                .With(e => e.BookType, bookType)
                .Create();
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WhenUnitOfWorkIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            IUnitOfWork unitOfWork = null;
            IValidator<Edition> validator = Mock.Of<IValidator<Edition>>();
            ILogger<EditionService> logger = Mock.Of<ILogger<EditionService>>();

            // Act
            Action act = () => new EditionService(unitOfWork, validator, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*unitOfWork*");
        }

        [TestMethod]
        public void Constructor_WhenValidatorIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var unitOfWork = Mock.Of<IUnitOfWork>();
            IValidator<Edition> validator = null;
            var logger = Mock.Of<ILogger<EditionService>>();

            // Act
            Action act = () => new EditionService(unitOfWork, validator, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*validator*");
        }

        [TestMethod]
        public void Constructor_WhenLoggerIsNull_ShouldNotThrow()
        {
            // Arrange
            var unitOfWork = Mock.Of<IUnitOfWork>();
            var validator = Mock.Of<IValidator<Edition>>();
            ILogger<EditionService> logger = null;

            // Act
            Action act = () => new EditionService(unitOfWork, validator, logger);

            // Assert
            act.Should().NotThrow();
        }

        #endregion

        #region CreateAsync Tests

        [TestMethod]
        public async Task CreateAsync_BookNotFound_ShouldReturnServiceFailure()
        {
            // Arrange
            var edition = CreateValidEdition();

            _bookRepositoryMock
                .Setup(r => r.ExistsAsync(edition.Book.Id))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(edition);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Book");
            result.ErrorMessage.Should().Contain("not found");
        }

        [TestMethod]
        public async Task CreateAsync_BookTypeNotFound_ShouldReturnServiceFailure()
        {
            // Arrange
            var edition = CreateValidEdition();

            _bookRepositoryMock
                .Setup(r => r.ExistsAsync(edition.Book.Id))
                .ReturnsAsync(true);

            _bookTypeRepositoryMock
                .Setup(r => r.ExistsAsync(edition.BookType.Id))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(edition);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("BookType");
            result.ErrorMessage.Should().Contain("not found");
        }

        [TestMethod]
        public async Task CreateAsync_ValidEdition_ShouldReturnSuccess()
        {
            // Arrange
            var edition = CreateValidEdition();
            var addedEdition = _fixture.Build<Edition>()
                .With(e => e.Id, edition.Id)
                .Create();

            _bookRepositoryMock.Setup(r => r.ExistsAsync(edition.Book.Id))
                .ReturnsAsync(true);
            _bookTypeRepositoryMock.Setup(r => r.ExistsAsync(edition.BookType.Id))
                .ReturnsAsync(true);
            _editionRepositoryMock.Setup(r => r.AddAsync(edition))
                .ReturnsAsync(addedEdition);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(edition);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be(addedEdition);
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.ValidationErrors.Should().BeEmpty();
        }

        [TestMethod]
        public async Task CreateAsync_ShouldCallRepositoryMethods()
        {
            // Arrange
            var edition = CreateValidEdition();

            _bookRepositoryMock.Setup(r => r.ExistsAsync(edition.Book.Id))
                .ReturnsAsync(true);
            _bookTypeRepositoryMock.Setup(r => r.ExistsAsync(edition.BookType.Id))
                .ReturnsAsync(true);
            _editionRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Edition>()))
                .ReturnsAsync(edition);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _service.CreateAsync(edition);

            // Assert
            _bookRepositoryMock.Verify(r => r.ExistsAsync(edition.Book.Id), Times.Once);
            _bookTypeRepositoryMock.Verify(r => r.ExistsAsync(edition.BookType.Id), Times.Once);
            _editionRepositoryMock.Verify(r => r.AddAsync(edition), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task CreateAsync_BookNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var edition = CreateValidEdition();
            _bookRepositoryMock.Setup(r => r.ExistsAsync(edition.Book.Id))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(edition);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Book with ID '{edition.Book.Id}' not found");
        }

        [TestMethod]
        public async Task CreateAsync_BookTypeNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var edition = CreateValidEdition();
            _bookRepositoryMock.Setup(r => r.ExistsAsync(edition.Book.Id))
                .ReturnsAsync(true);
            _bookTypeRepositoryMock.Setup(r => r.ExistsAsync(edition.BookType.Id))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(edition);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"BookType with ID '{edition.BookType.Id}' not found");
        }

        [TestMethod]
        public async Task CreateAsync_ValidationFailure_ShouldReturnValidationErrors()
        {
            // Arrange
            var edition = CreateValidEdition();
            edition.NumberOfPages = 10;

            // Act
            var result = await _service.CreateAsync(edition);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().Contain(item => item.Contains("Number of pages"));
        }

        [TestMethod]
        public async Task CreateAsync_FuturePublicationDate_ShouldReturnValidationError()
        {
            // Arrange
            var edition = CreateValidEdition();
            edition.PublicationDate = DateTime.Now.AddMonths(6);

            // Act
            var result = await _service.CreateAsync(edition);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().Contain(item => item.Contains("Publication date cannot be in the future"));
        }

        #endregion

        #region GetByIdAsync Tests

        [TestMethod]
        public async Task GetByIdAsync_EditionExists_ShouldReturnEdition()
        {
            // Arrange
            var editionId = Guid.NewGuid();
            var edition = _fixture.Build<Edition>()
                .With(e => e.Id, editionId)
                .Create();

            _editionRepositoryMock.Setup(r => r.GetByIdAsync(editionId))
                .ReturnsAsync(edition);

            // Act
            var result = await _service.GetByIdAsync(editionId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be(edition);
        }

        [TestMethod]
        public async Task GetByIdAsync_EditionNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var editionId = Guid.NewGuid();
            _editionRepositoryMock.Setup(r => r.GetByIdAsync(editionId))
                .ReturnsAsync((Edition)null);

            // Act
            var result = await _service.GetByIdAsync(editionId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Edition with ID '{editionId}' not found");
        }

        #endregion

        #region GetAllAsync Tests

        [TestMethod]
        public async Task GetAllAsync_EditionsExist_ShouldReturnAllEditions()
        {
            // Arrange
            var editions = _fixture.CreateMany<Edition>(3).ToList();
            _editionRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(editions);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(editions);
        }

        [TestMethod]
        public async Task GetAllAsync_NoEditions_ShouldReturnEmptyList()
        {
            // Arrange
            _editionRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Edition>());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region UpdateAsync Tests

        [TestMethod]
        public async Task UpdateAsync_ValidEdition_ShouldReturnSuccess()
        {
            // Arrange
            var edition = CreateValidEdition();

            _editionRepositoryMock.Setup(r => r.ExistsAsync(edition.Id))
                .ReturnsAsync(true);
            _bookRepositoryMock.Setup(r => r.ExistsAsync(edition.Book.Id))
                .ReturnsAsync(true);
            _bookTypeRepositoryMock.Setup(r => r.ExistsAsync(edition.BookType.Id))
                .ReturnsAsync(true);
            _editionRepositoryMock.Setup(r => r.UpdateAsync(edition))
                .ReturnsAsync(edition);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.UpdateAsync(edition);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [TestMethod]
        public async Task UpdateAsync_EditionNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var edition = CreateValidEdition();
            _editionRepositoryMock.Setup(r => r.ExistsAsync(edition.Id))
                .ReturnsAsync(false);

            // Act
            var result = await _service.UpdateAsync(edition);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Edition with ID '{edition.Id}' not found");
        }

        [TestMethod]
        public async Task UpdateAsync_BookNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var edition = CreateValidEdition();

            _editionRepositoryMock.Setup(r => r.ExistsAsync(edition.Id))
                .ReturnsAsync(true);
            _bookRepositoryMock.Setup(r => r.ExistsAsync(edition.Book.Id))
                .ReturnsAsync(false);

            // Act
            var result = await _service.UpdateAsync(edition);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Book with ID '{edition.Book.Id}' not found");
        }

        [TestMethod]
        public async Task UpdateAsync_BookTypeNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var edition = CreateValidEdition();

            _editionRepositoryMock.Setup(r => r.ExistsAsync(edition.Id))
                .ReturnsAsync(true);
            _bookRepositoryMock.Setup(r => r.ExistsAsync(edition.Book.Id))
                .ReturnsAsync(true);
            _bookTypeRepositoryMock.Setup(r => r.ExistsAsync(edition.BookType.Id))
                .ReturnsAsync(false);

            // Act
            var result = await _service.UpdateAsync(edition);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"BookType with ID '{edition.BookType.Id}' not found");
        }

        [TestMethod]
        public async Task UpdateAsync_ValidationFailure_ShouldReturnValidationErrors()
        {
            // Arrange
            var edition = CreateValidEdition();
            edition.NumberOfPages = 10000;

            _editionRepositoryMock.Setup(r => r.ExistsAsync(edition.Id))
                .ReturnsAsync(true);
            _bookRepositoryMock.Setup(r => r.ExistsAsync(edition.Book.Id))
                .ReturnsAsync(true);
            _bookTypeRepositoryMock.Setup(r => r.ExistsAsync(edition.BookType.Id))
                .ReturnsAsync(true);

            // Act
            var result = await _service.UpdateAsync(edition);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().Contain(item => item.Contains("Number of pages"));
        }

        #endregion

        #region DeleteAsync Tests (Adjusted Version Without Transaction)

        [TestMethod]
        public async Task DeleteAsync_EditionWithoutCopies_ShouldReturnSuccess()
        {
            // Arrange
            var editionId = Guid.NewGuid();
            var edition = CreateValidEdition();

            _editionRepositoryMock.Setup(r => r.GetByIdAsync(editionId))
                .ReturnsAsync(edition);
            _editionRepositoryMock.Setup(r => r.HasCopiesAsync(editionId))
                .ReturnsAsync(false);
            _editionRepositoryMock.Setup(r => r.DeleteAsync(editionId))
                .ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.DeleteAsync(editionId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
            _editionRepositoryMock.Verify(r => r.DeleteAsync(editionId), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task DeleteAsync_EditionNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var editionId = Guid.NewGuid();
            _editionRepositoryMock.Setup(r => r.GetByIdAsync(editionId))
                .ReturnsAsync((Edition)null);

            // Act
            var result = await _service.DeleteAsync(editionId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Edition with ID '{editionId}' not found");
            _editionRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [TestMethod]
        public async Task DeleteAsync_EditionHasCopies_ShouldReturnBusinessRuleError()
        {
            // Arrange
            var editionId = Guid.NewGuid();
            var edition = CreateValidEdition();

            _editionRepositoryMock.Setup(r => r.GetByIdAsync(editionId))
                .ReturnsAsync(edition);
            _editionRepositoryMock.Setup(r => r.HasCopiesAsync(editionId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(editionId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Cannot delete an edition that has physical copies");
            _editionRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [TestMethod]
        public async Task DeleteAsync_DeleteFails_ShouldReturnFailure()
        {
            // Arrange
            var editionId = Guid.NewGuid();
            var edition = CreateValidEdition();

            _editionRepositoryMock.Setup(r => r.GetByIdAsync(editionId))
                .ReturnsAsync(edition);
            _editionRepositoryMock.Setup(r => r.HasCopiesAsync(editionId))
                .ReturnsAsync(false);
            _editionRepositoryMock.Setup(r => r.DeleteAsync(editionId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteAsync(editionId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Failed to delete edition");
            _editionRepositoryMock.Verify(r => r.DeleteAsync(editionId), Times.Once);
        }

        #endregion

        #region ExistsAsync Tests

        [TestMethod]
        public async Task ExistsAsync_EditionExists_ShouldReturnTrue()
        {
            // Arrange
            var editionId = Guid.NewGuid();
            _editionRepositoryMock.Setup(r => r.ExistsAsync(editionId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ExistsAsync(editionId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [TestMethod]
        public async Task ExistsAsync_EditionDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var editionId = Guid.NewGuid();
            _editionRepositoryMock.Setup(r => r.ExistsAsync(editionId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.ExistsAsync(editionId);

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
            var edition = CreateValidEdition();
            _bookRepositoryMock.Setup(r => r.ExistsAsync(edition.Book.Id))
                .ReturnsAsync(true);
            _bookTypeRepositoryMock.Setup(r => r.ExistsAsync(edition.BookType.Id))
                .ReturnsAsync(true);
            _editionRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Edition>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act
            var result = await _service.CreateAsync(edition);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred");
        }

        [TestMethod]
        public async Task GetByIdAsync_RepositoryThrowsException_ShouldReturnFailureWithErrorMessage()
        {
            // Arrange
            var editionId = Guid.NewGuid();
            _editionRepositoryMock.Setup(r => r.GetByIdAsync(editionId))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act
            var result = await _service.GetByIdAsync(editionId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred");
        }

        [TestMethod]
        public async Task DeleteAsync_WhenBusinessRuleExceptionThrown_ShouldReturnFailureWithBusinessRuleMessage()
        {
            // Arrange
            var editionId = Guid.NewGuid();
            var edition = CreateValidEdition();

            _editionRepositoryMock.Setup(r => r.GetByIdAsync(editionId))
                .ReturnsAsync(edition);
            _editionRepositoryMock.Setup(r => r.HasCopiesAsync(editionId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(editionId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Cannot delete an edition that has physical copies");
        }

        [TestMethod]
        public async Task DeleteAsync_WhenNotFoundExceptionThrown_ShouldReturnFailureWithNotFoundMessage()
        {
            // Arrange
            var editionId = Guid.NewGuid();
            _editionRepositoryMock.Setup(r => r.GetByIdAsync(editionId))
                .ReturnsAsync((Edition)null);

            // Act
            var result = await _service.DeleteAsync(editionId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Edition with ID '{editionId}' not found");
        }

        #endregion

        #region Edge Cases and Boundary Tests

        [TestMethod]
        public async Task CreateAsync_WithMinimumValidPages_ShouldSucceed()
        {
            // Arrange
            var edition = CreateValidEdition();
            edition.NumberOfPages = 30;

            _bookRepositoryMock.Setup(r => r.ExistsAsync(edition.Book.Id))
                .ReturnsAsync(true);
            _bookTypeRepositoryMock.Setup(r => r.ExistsAsync(edition.BookType.Id))
                .ReturnsAsync(true);
            _editionRepositoryMock.Setup(r => r.AddAsync(edition))
                .ReturnsAsync(edition);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(edition);

            // Assert
            result.Success.Should().BeTrue();
        }

        [TestMethod]
        public async Task CreateAsync_WithMaximumValidPages_ShouldSucceed()
        {
            // Arrange
            var edition = CreateValidEdition();
            edition.NumberOfPages = 5000;

            _bookRepositoryMock.Setup(r => r.ExistsAsync(edition.Book.Id))
                .ReturnsAsync(true);
            _bookTypeRepositoryMock.Setup(r => r.ExistsAsync(edition.BookType.Id))
                .ReturnsAsync(true);
            _editionRepositoryMock.Setup(r => r.AddAsync(edition))
                .ReturnsAsync(edition);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(edition);

            // Assert
            result.Success.Should().BeTrue();
        }

        [TestMethod]
        public async Task CreateAsync_WithNullBook_ShouldReturnValidationError()
        {
            // Arrange
            var edition = CreateValidEdition();
            edition.Book = null;

            // Act
            var result = await _service.CreateAsync(edition);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().Contain(item => item.Contains("Book is required"));
        }

        [TestMethod]
        public async Task CreateAsync_WithNullBookType_ShouldReturnValidationError()
        {
            // Arrange
            var edition = CreateValidEdition();
            edition.BookType = null;

            // Act
            var result = await _service.CreateAsync(edition);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().Contain(item => item.Contains("Book type is required"));
        }

        #endregion
    }
}
