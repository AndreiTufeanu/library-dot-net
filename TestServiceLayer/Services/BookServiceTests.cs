using AutoFixture;
using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLayer.Exceptions;
using ServiceLayer.ServiceContracts.HelperServiceContracts;
using ServiceLayer.Services;
using ServiceLayer.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServiceLayer.Services
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class BookServiceTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IBookHelperService> _bookHelperServiceMock;
        private readonly Mock<ILogger<BookService>> _loggerMock;
        private readonly IValidator<Book> _validator;
        private readonly BookService _service;

        private readonly Mock<IBookRepository> _bookRepositoryMock;
        private readonly Mock<IDomainRepository> _domainRepositoryMock;

        public BookServiceTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _bookHelperServiceMock = new Mock<IBookHelperService>();
            _loggerMock = new Mock<ILogger<BookService>>();

            // Setup repository mocks
            _bookRepositoryMock = new Mock<IBookRepository>();
            _domainRepositoryMock = new Mock<IDomainRepository>();

            // Setup unit of work to return repository mocks
            _unitOfWorkMock.Setup(u => u.Books).Returns(_bookRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.Domains).Returns(_domainRepositoryMock.Object);

            _validator = new BookValidator();
            _service = new BookService(_unitOfWorkMock.Object, _validator, _bookHelperServiceMock.Object, _loggerMock.Object);
        }

        private Book CreateValidBook(int initialCopies = 10)
        {
            var domain = _fixture.Build<Domain>().Create();

            var validBook = _fixture.Build<Book>()
                .FromFactory(() => new Book(initialCopies: initialCopies))
                .With(b => b.Title, "The Great Gatsby")
                .Create();

            validBook.Domains.Add(domain);

            return validBook;
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WhenUnitOfWorkIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            IUnitOfWork unitOfWork = null;
            IValidator<Book> validator = Mock.Of<IValidator<Book>>();
            IBookHelperService bookHelperService = Mock.Of<IBookHelperService>();
            ILogger<BookService> logger = Mock.Of<ILogger<BookService>>();

            // Act
            Action act = () => new BookService(unitOfWork, validator, bookHelperService, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*unitOfWork*");
        }

        [TestMethod]
        public void Constructor_WhenValidatorIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var unitOfWork = Mock.Of<IUnitOfWork>();
            IValidator<Book> validator = null;
            var bookHelperService = Mock.Of<IBookHelperService>();
            var logger = Mock.Of<ILogger<BookService>>();

            // Act
            Action act = () => new BookService(unitOfWork, validator, bookHelperService, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*validator*");
        }

        [TestMethod]
        public void Constructor_WhenBookHelperServiceIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var unitOfWork = Mock.Of<IUnitOfWork>();
            var validator = Mock.Of<IValidator<Book>>();
            IBookHelperService bookHelperService = null;
            var logger = Mock.Of<ILogger<BookService>>();

            // Act
            Action act = () => new BookService(unitOfWork, validator, bookHelperService, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*bookHelperService*");
        }

        [TestMethod]
        public void Constructor_WhenLoggerIsNull_ShouldNotThrow()
        {
            // Arrange
            var unitOfWork = Mock.Of<IUnitOfWork>();
            var validator = Mock.Of<IValidator<Book>>();
            var bookHelperService = Mock.Of<IBookHelperService>();
            ILogger<BookService> logger = null;

            // Act
            Action act = () => new BookService(unitOfWork, validator, bookHelperService, logger);

            // Assert
            act.Should().NotThrow();
        }

        #endregion

        #region CreateAsync Tests

        [TestMethod]
        public async Task CreateAsync_ValidBook_ShouldReturnSuccess()
        {
            // Arrange
            var book = CreateValidBook();
            var addedBook = _fixture.Build<Book>()
                .With(b => b.Id, book.Id)
                .Create();

            _bookHelperServiceMock.Setup(s => s.ValidateMaxDomainsPerBookAsync(book.Domains))
                .Returns(Task.CompletedTask);
            _bookRepositoryMock.Setup(r => r.AddAsync(book))
                .ReturnsAsync(addedBook);

            // Act
            var result = await _service.CreateAsync(book);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be(addedBook);
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.ValidationErrors.Should().BeEmpty();
        }

        [TestMethod]
        public async Task CreateAsync_ShouldCallHelperServiceAndRepositoryMethods()
        {
            // Arrange
            var book = CreateValidBook();

            _bookHelperServiceMock.Setup(s => s.ValidateMaxDomainsPerBookAsync(book.Domains))
                .Returns(Task.CompletedTask);
            _bookRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Book>()))
                .ReturnsAsync(book);

            // Act
            await _service.CreateAsync(book);

            // Assert
            _bookHelperServiceMock.Verify(s => s.ValidateMaxDomainsPerBookAsync(book.Domains), Times.Once);
            _bookRepositoryMock.Verify(r => r.AddAsync(book), Times.Once);
        }

        [TestMethod]
        public async Task CreateAsync_WhenHelperServiceThrowsValidationError_ShouldReturnValidationErrors()
        {
            // Arrange
            var book = CreateValidBook();
            var validationError = "A book cannot belong to more than 5 domains. Current count: 7";

            _bookHelperServiceMock.Setup(s => s.ValidateMaxDomainsPerBookAsync(book.Domains))
                .ThrowsAsync(new AggregateValidationException(validationError));

            // Act
            var result = await _service.CreateAsync(book);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Be(validationError);
        }

        [TestMethod]
        public async Task CreateAsync_ValidationFailure_ShouldReturnValidationErrors()
        {
            // Arrange
            var book = CreateValidBook();
            book.Title = "A";

            // Act
            var result = await _service.CreateAsync(book);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().Contain(item => item.Contains("Title must be between"));
        }

        #endregion

        #region GetByIdAsync Tests

        [TestMethod]
        public async Task GetByIdAsync_BookExists_ShouldReturnBook()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var book = _fixture.Build<Book>()
                .With(b => b.Id, bookId)
                .Create();

            _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId))
                .ReturnsAsync(book);

            // Act
            var result = await _service.GetByIdAsync(bookId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be(book);
        }

        [TestMethod]
        public async Task GetByIdAsync_BookNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId))
                .ReturnsAsync((Book)null);

            // Act
            var result = await _service.GetByIdAsync(bookId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Book with ID '{bookId}' not found");
        }

        #endregion

        #region GetAllAsync Tests

        [TestMethod]
        public async Task GetAllAsync_BooksExist_ShouldReturnAllBooks()
        {
            // Arrange
            var books = _fixture.CreateMany<Book>(3).ToList();
            _bookRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(books);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(books);
        }

        [TestMethod]
        public async Task GetAllAsync_NoBooks_ShouldReturnEmptyList()
        {
            // Arrange
            _bookRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Book>());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region UpdateAsync Tests

        [TestMethod]
        public async Task UpdateAsync_ValidBook_ShouldReturnSuccess()
        {
            // Arrange
            var book = CreateValidBook();

            _bookRepositoryMock.Setup(r => r.ExistsAsync(book.Id))
                .ReturnsAsync(true);
            _bookHelperServiceMock.Setup(s => s.ValidateMaxDomainsPerBookAsync(book.Domains))
                .Returns(Task.CompletedTask);
            _bookRepositoryMock.Setup(r => r.UpdateAsync(book))
                .ReturnsAsync(book);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.UpdateAsync(book);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [TestMethod]
        public async Task UpdateAsync_BookNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var book = CreateValidBook();
            _bookRepositoryMock.Setup(r => r.ExistsAsync(book.Id))
                .ReturnsAsync(false);

            // Act
            var result = await _service.UpdateAsync(book);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Book with ID '{book.Id}' not found");
        }

        [TestMethod]
        public async Task UpdateAsync_ShouldCallHelperServiceForDomainValidation()
        {
            // Arrange
            var book = CreateValidBook();

            _bookRepositoryMock.Setup(r => r.ExistsAsync(book.Id))
                .ReturnsAsync(true);
            _bookHelperServiceMock.Setup(s => s.ValidateMaxDomainsPerBookAsync(book.Domains))
                .Returns(Task.CompletedTask);
            _bookRepositoryMock.Setup(r => r.UpdateAsync(book))
                .ReturnsAsync(book);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _service.UpdateAsync(book);

            // Assert
            _bookHelperServiceMock.Verify(s => s.ValidateMaxDomainsPerBookAsync(book.Domains), Times.Once);
        }

        #endregion

        #region DeleteAsync Tests (Without Transactions)

        [TestMethod]
        public async Task DeleteAsync_BookWithoutPhysicalCopies_ShouldReturnSuccess()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var book = CreateValidBook();

            _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId))
                .ReturnsAsync(book);
            _bookRepositoryMock.Setup(r => r.HasPhysicalCopiesAsync(bookId))
                .ReturnsAsync(false);
            _bookRepositoryMock.Setup(r => r.DeleteAsync(bookId))
                .ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.DeleteAsync(bookId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
            _bookRepositoryMock.Verify(r => r.DeleteAsync(bookId), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task DeleteAsync_BookNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId))
                .ReturnsAsync((Book)null);

            // Act
            var result = await _service.DeleteAsync(bookId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Book with ID '{bookId}' not found");
            _bookRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task DeleteAsync_BookHasPhysicalCopies_ShouldReturnBusinessRuleError()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var book = CreateValidBook();

            _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId))
                .ReturnsAsync(book);
            _bookRepositoryMock.Setup(r => r.HasPhysicalCopiesAsync(bookId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(bookId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Cannot delete a book that has physical copies");
            _bookRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task DeleteAsync_DeleteFails_ShouldReturnFailure()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var book = CreateValidBook();

            _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId))
                .ReturnsAsync(book);
            _bookRepositoryMock.Setup(r => r.HasPhysicalCopiesAsync(bookId))
                .ReturnsAsync(false);
            _bookRepositoryMock.Setup(r => r.DeleteAsync(bookId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteAsync(bookId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Failed to delete book");
            _bookRepositoryMock.Verify(r => r.DeleteAsync(bookId), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        #endregion

        #region ExistsAsync Tests

        [TestMethod]
        public async Task ExistsAsync_BookExists_ShouldReturnTrue()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            _bookRepositoryMock.Setup(r => r.ExistsAsync(bookId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ExistsAsync(bookId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [TestMethod]
        public async Task ExistsAsync_BookDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            _bookRepositoryMock.Setup(r => r.ExistsAsync(bookId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.ExistsAsync(bookId);

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
            var book = CreateValidBook();

            _bookHelperServiceMock.Setup(s => s.ValidateMaxDomainsPerBookAsync(book.Domains))
                .Returns(Task.CompletedTask);
            _bookRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Book>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act
            var result = await _service.CreateAsync(book);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred");
        }

        [TestMethod]
        public async Task UpdateAsync_RepositoryThrowsException_ShouldReturnFailureWithErrorMessage()
        {
            // Arrange
            var book = CreateValidBook();

            _bookRepositoryMock.Setup(r => r.ExistsAsync(book.Id))
                .ReturnsAsync(true);
            _bookHelperServiceMock.Setup(s => s.ValidateMaxDomainsPerBookAsync(book.Domains))
                .Returns(Task.CompletedTask);
            _bookRepositoryMock.Setup(r => r.UpdateAsync(book))
                .ThrowsAsync(new InvalidOperationException("Database constraint violation"));

            // Act
            var result = await _service.UpdateAsync(book);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred");
        }

        #endregion

        #region Edge Cases and Boundary Tests

        [TestMethod]
        public async Task CreateAsync_WithMinimumValidTitleLength_ShouldSucceed()
        {
            // Arrange
            var book = CreateValidBook();
            book.Title = "AB";

            _bookHelperServiceMock.Setup(s => s.ValidateMaxDomainsPerBookAsync(book.Domains))
                .Returns(Task.CompletedTask);
            _bookRepositoryMock.Setup(r => r.AddAsync(book))
                .ReturnsAsync(book);

            // Act
            var result = await _service.CreateAsync(book);

            // Assert
            result.Success.Should().BeTrue();
        }

        [TestMethod]
        public async Task CreateAsync_WithMaximumValidTitleLength_ShouldSucceed()
        {
            // Arrange
            var book = CreateValidBook();
            book.Title = new string('A', 300);

            _bookHelperServiceMock.Setup(s => s.ValidateMaxDomainsPerBookAsync(book.Domains))
                .Returns(Task.CompletedTask);
            _bookRepositoryMock.Setup(r => r.AddAsync(book))
                .ReturnsAsync(book);

            // Act
            var result = await _service.CreateAsync(book);

            // Assert
            result.Success.Should().BeTrue();
        }

        [TestMethod]
        public async Task CreateAsync_WithMinimumValidInitialCopies_ShouldSucceed()
        {
            // Arrange
            var book = CreateValidBook(initialCopies: 1);

            _bookHelperServiceMock.Setup(s => s.ValidateMaxDomainsPerBookAsync(book.Domains))
                .Returns(Task.CompletedTask);
            _bookRepositoryMock.Setup(r => r.AddAsync(book))
                .ReturnsAsync(book);

            // Act
            var result = await _service.CreateAsync(book);

            // Assert
            result.Success.Should().BeTrue();
        }

        [TestMethod]
        public async Task CreateAsync_WithLargeInitialCopies_ShouldSucceed()
        {
            // Arrange
            var book = CreateValidBook(initialCopies: 10000);

            _bookHelperServiceMock.Setup(s => s.ValidateMaxDomainsPerBookAsync(book.Domains))
                .Returns(Task.CompletedTask);
            _bookRepositoryMock.Setup(r => r.AddAsync(book))
                .ReturnsAsync(book);

            // Act
            var result = await _service.CreateAsync(book);

            // Assert
            result.Success.Should().BeTrue();
        }

        [TestMethod]
        public async Task CreateAsync_WithEmptyTitle_ShouldReturnValidationError()
        {
            // Arrange
            var book = CreateValidBook();
            book.Title = "";

            // Act
            var result = await _service.CreateAsync(book);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain("Title is required");
        }

        [TestMethod]
        public async Task CreateAsync_WithNullTitle_ShouldReturnValidationError()
        {
            // Arrange
            var book = CreateValidBook();
            book.Title = null;

            // Act
            var result = await _service.CreateAsync(book);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain("Title is required");
        }

        #endregion
    }
}
