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
    public class AuthorServiceTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<AuthorService>> _loggerMock;
        private readonly AuthorService _service;

        private readonly Mock<IAuthorRepository> _authorRepositoryMock;

        public AuthorServiceTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<AuthorService>>();

            _authorRepositoryMock = new Mock<IAuthorRepository>();
            _unitOfWorkMock.Setup(u => u.Authors).Returns(_authorRepositoryMock.Object);

            _service = new AuthorService(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        private Author CreateValidAuthor()
        {
            return _fixture.Build<Author>()
                .With(a => a.FirstName, "John")
                .With(a => a.LastName, "Doe")
                .Create();
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WhenUnitOfWorkIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            IUnitOfWork unitOfWork = null;
            IValidator<Author> validator = Mock.Of<IValidator<Author>>();
            ILogger<AuthorService> logger = Mock.Of<ILogger<AuthorService>>();

            // Act
            Action act = () => new AuthorService(unitOfWork, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        #endregion

        #region CreateAsync Tests

        [TestMethod]
        public async Task CreateAsync_ValidAuthor_ShouldReturnSuccess()
        {
            // Arrange
            var author = CreateValidAuthor();
            var addedAuthor = _fixture.Build<Author>()
                .With(a => a.Id, author.Id)
                .Create();

            _authorRepositoryMock.Setup(r => r.FindByNameAsync(author.FirstName, author.LastName))
                .ReturnsAsync((Author)null);
            _authorRepositoryMock.Setup(r => r.AddAsync(author))
                .ReturnsAsync(addedAuthor);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(author);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be(addedAuthor);
        }

        [TestMethod]
        public async Task CreateAsync_ValidAuthor_ShouldHaveNoErrors()
        {
            // Arrange
            var author = CreateValidAuthor();
            var addedAuthor = _fixture.Build<Author>()
                .With(a => a.Id, author.Id)
                .Create();

            _authorRepositoryMock.Setup(r => r.FindByNameAsync(author.FirstName, author.LastName))
                .ReturnsAsync((Author)null);
            _authorRepositoryMock.Setup(r => r.AddAsync(author))
                .ReturnsAsync(addedAuthor);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(author);

            // Assert
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.ValidationErrors.Should().BeEmpty();
        }

        [TestMethod]
        public async Task CreateAsync_ValidAuthor_ShouldCallRepositoryMethods()
        {
            // Arrange
            var author = CreateValidAuthor();

            _authorRepositoryMock.Setup(r => r.FindByNameAsync(author.FirstName, author.LastName))
                .ReturnsAsync((Author)null);
            _authorRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Author>()))
                .ReturnsAsync(author);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _service.CreateAsync(author);

            // Assert
            _authorRepositoryMock.Verify(r => r.FindByNameAsync(author.FirstName, author.LastName), Times.Once);
            _authorRepositoryMock.Verify(r => r.AddAsync(author), Times.Once);
        }

        [TestMethod]
        public async Task CreateAsync_DuplicateAuthor_ShouldReturnValidationError()
        {
            // Arrange
            var author = CreateValidAuthor();
            var existingAuthor = CreateValidAuthor();

            _authorRepositoryMock.Setup(r => r.FindByNameAsync(author.FirstName, author.LastName))
                .ReturnsAsync(existingAuthor);

            // Act
            var result = await _service.CreateAsync(author);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().Contain(item => item.Contains("already exists"));
        }

        [TestMethod]
        public async Task CreateAsync_ValidationFailure_ShouldReturnValidationErrors()
        {
            // Arrange
            var author = CreateValidAuthor();
            author.FirstName = "J";

            // Act
            var result = await _service.CreateAsync(author);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().Contain(item => item.Contains("First Name must have between"));
        }

        #endregion

        #region GetByIdAsync Tests

        [TestMethod]
        public async Task GetByIdAsync_AuthorExists_ShouldReturnAuthor()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var author = _fixture.Build<Author>()
                .With(a => a.Id, authorId)
                .Create();

            _authorRepositoryMock.Setup(r => r.GetByIdAsync(authorId))
                .ReturnsAsync(author);

            // Act
            var result = await _service.GetByIdAsync(authorId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be(author);
        }

        [TestMethod]
        public async Task GetByIdAsync_AuthorNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            _authorRepositoryMock.Setup(r => r.GetByIdAsync(authorId))
                .ReturnsAsync((Author)null);

            // Act
            var result = await _service.GetByIdAsync(authorId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Author with ID '{authorId}' not found");
        }

        #endregion

        #region GetAllAsync Tests

        [TestMethod]
        public async Task GetAllAsync_AuthorsExist_ShouldReturnAllAuthors()
        {
            // Arrange
            var authors = _fixture.CreateMany<Author>(3).ToList();
            _authorRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(authors);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(authors);
        }

        [TestMethod]
        public async Task GetAllAsync_NoAuthors_ShouldReturnEmptyList()
        {
            // Arrange
            _authorRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Author>());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region UpdateAsync Tests

        [TestMethod]
        public async Task UpdateAsync_ValidAuthor_ShouldReturnSuccess()
        {
            // Arrange
            var author = CreateValidAuthor();

            _authorRepositoryMock.Setup(r => r.ExistsAsync(author.Id))
                .ReturnsAsync(true);
            _authorRepositoryMock.Setup(r => r.FindByNameAsync(author.FirstName, author.LastName))
                .ReturnsAsync((Author)null);
            _authorRepositoryMock.Setup(r => r.UpdateAsync(author))
                .ReturnsAsync(author);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.UpdateAsync(author);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [TestMethod]
        public async Task UpdateAsync_AuthorNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var author = CreateValidAuthor();
            _authorRepositoryMock.Setup(r => r.ExistsAsync(author.Id))
                .ReturnsAsync(false);

            // Act
            var result = await _service.UpdateAsync(author);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Author with ID '{author.Id}' not found");
        }

        [TestMethod]
        public async Task UpdateAsync_DuplicateAuthorName_ShouldReturnValidationError()
        {
            // Arrange
            var author = CreateValidAuthor();
            var existingAuthor = _fixture.Build<Author>()
                .Create();

            _authorRepositoryMock.Setup(r => r.ExistsAsync(author.Id))
                .ReturnsAsync(true);
            _authorRepositoryMock.Setup(r => r.FindByNameAsync(author.FirstName, author.LastName))
                .ReturnsAsync(existingAuthor);

            // Act
            var result = await _service.UpdateAsync(author);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().Contain(item => item.Contains("Another author with name"));
        }

        #endregion

        #region DeleteAsync Tests

        [TestMethod]
        public async Task DeleteAsync_AuthorWithoutBooks_ShouldReturnSuccess()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var author = CreateValidAuthor();

            _authorRepositoryMock.Setup(r => r.GetByIdAsync(authorId))
                .ReturnsAsync(author);
            _authorRepositoryMock.Setup(r => r.HasBooksAsync(authorId))
                .ReturnsAsync(false);
            _authorRepositoryMock.Setup(r => r.DeleteAsync(authorId))
                .ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);
            _unitOfWorkMock.Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteAsync(authorId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [TestMethod]
        public async Task DeleteAsync_AuthorNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            _authorRepositoryMock.Setup(r => r.GetByIdAsync(authorId))
                .ReturnsAsync((Author)null);

            // Act
            var result = await _service.DeleteAsync(authorId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Author with ID '{authorId}' not found");
        }

        [TestMethod]
        public async Task DeleteAsync_AuthorHasBooks_ShouldReturnBusinessRuleError()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var author = CreateValidAuthor();

            _authorRepositoryMock.Setup(r => r.GetByIdAsync(authorId))
                .ReturnsAsync(author);
            _authorRepositoryMock.Setup(r => r.HasBooksAsync(authorId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(authorId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Cannot delete an author that has associated books");
        }

        [TestMethod]
        public async Task DeleteAsync_TransactionRollsBackOnFailure_ShouldReturnFailure()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var author = CreateValidAuthor();

            _authorRepositoryMock.Setup(r => r.GetByIdAsync(authorId))
                .ReturnsAsync(author);
            _authorRepositoryMock.Setup(r => r.HasBooksAsync(authorId))
                .ReturnsAsync(false);
            _authorRepositoryMock.Setup(r => r.DeleteAsync(authorId))
                .ReturnsAsync(false);
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteAsync(authorId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Failed to delete author");
        }

        [TestMethod]
        public async Task DeleteAsync_WhenExceptionOccursDuringTransaction_ShouldCallRollbackAndRethrow()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var author = CreateValidAuthor();

            _authorRepositoryMock.Setup(r => r.GetByIdAsync(authorId))
                .ReturnsAsync(author);
            _authorRepositoryMock.Setup(r => r.HasBooksAsync(authorId))
                .ReturnsAsync(false);
            _authorRepositoryMock.Setup(r => r.DeleteAsync(authorId))
                .ThrowsAsync(new InvalidOperationException("Database constraint violation"));

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.RollbackAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteAsync(authorId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred");
        }

        #endregion

        #region ExistsAsync Tests

        [TestMethod]
        public async Task ExistsAsync_AuthorExists_ShouldReturnTrue()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            _authorRepositoryMock.Setup(r => r.ExistsAsync(authorId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ExistsAsync(authorId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [TestMethod]
        public async Task ExistsAsync_AuthorDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            _authorRepositoryMock.Setup(r => r.ExistsAsync(authorId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.ExistsAsync(authorId);

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
            var author = CreateValidAuthor();
            _authorRepositoryMock.Setup(r => r.FindByNameAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act
            var result = await _service.CreateAsync(author);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred");
        }

        [TestMethod]
        public async Task ServiceMethod_WhenBusinessRuleExceptionThrown_ShouldReturnFailureWithBusinessRuleMessage()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var author = CreateValidAuthor();

            _authorRepositoryMock.Setup(r => r.GetByIdAsync(authorId))
                .ReturnsAsync(author);
            _authorRepositoryMock.Setup(r => r.HasBooksAsync(authorId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(authorId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Cannot delete an author that has associated books");
        }

        #endregion
    }
}
