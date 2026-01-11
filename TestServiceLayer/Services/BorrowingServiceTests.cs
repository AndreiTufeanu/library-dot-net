using AutoFixture;
using AutoFixture.AutoMoq;
using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLayer.Exceptions;
using ServiceLayer.ServiceContracts;
using ServiceLayer.ServiceContracts.HelperServiceContracts;
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
    public class BorrowingServiceTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IBorrowingHelperService> _borrowingHelperServiceMock;
        private readonly Mock<ILogger<BorrowingService>> _loggerMock;
        private readonly Mock<IConfigurationSettingService> _configServiceMock;
        private readonly BorrowingService _service;

        private readonly Mock<IBorrowingRepository> _borrowingRepositoryMock;
        private readonly Mock<IReaderRepository> _readerRepositoryMock;
        private readonly Mock<ILibrarianRepository> _librarianRepositoryMock;
        private readonly Mock<IBookCopyRepository> _bookCopyRepositoryMock;
        private readonly IValidator<Borrowing> _validator;

        public BorrowingServiceTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _borrowingHelperServiceMock = new Mock<IBorrowingHelperService>();
            _loggerMock = new Mock<ILogger<BorrowingService>>();
            _configServiceMock = new Mock<IConfigurationSettingService>();

            _borrowingRepositoryMock = new Mock<IBorrowingRepository>();
            _readerRepositoryMock = new Mock<IReaderRepository>();
            _librarianRepositoryMock = new Mock<ILibrarianRepository>();
            _bookCopyRepositoryMock = new Mock<IBookCopyRepository>();

            _unitOfWorkMock.Setup(u => u.Borrowings).Returns(_borrowingRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.Readers).Returns(_readerRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.Librarians).Returns(_librarianRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.BookCopies).Returns(_bookCopyRepositoryMock.Object);

            _validator = new BorrowingValidator();
            _service = new BorrowingService(_unitOfWorkMock.Object, _validator, _borrowingHelperServiceMock.Object, _loggerMock.Object, _configServiceMock.Object);
        }

        private BookCopy CreateBookCopy(bool isLectureRoomOnly = false)
        {
            var edition = _fixture.Build<Edition>()
                .With(e => e.Book, CreateBook())
                .Create();

            return _fixture.Build<BookCopy>()
                .FromFactory(() => new BookCopy(isLectureRoomOnly: isLectureRoomOnly))
                .With(c => c.Edition, edition)
                .Create();
        }

        private Book CreateBook()
        {
            return _fixture.Build<Book>()
                .FromFactory(() => new Book(initialCopies: 10))
                .Create();
        }

        private Reader CreateReader()
        {
            return _fixture.Create<Reader>();
        }

        private Librarian CreateLibrarian()
        {
            return _fixture.Create<Librarian>();
        }

        private Borrowing CreateValidBorrowing(Reader reader = null, Librarian librarian = null, List<BookCopy> bookCopies = null)
        {
            var borrowing = _fixture.Build<Borrowing>()
                .With(b => b.BorrowDate, DateTime.Now.Date)
                .With(b => b.DueDate, DateTime.Now.Date.AddDays(30))
                .With(b => b.ReturnDate, (DateTime?)null)
                .Without(b => b.ExtensionDays)
                .Create();

            borrowing.Reader = reader ?? CreateReader();
            borrowing.Librarian = librarian ?? CreateLibrarian();

            if (bookCopies != null)
            {
                foreach (var copy in bookCopies)
                {
                    borrowing.BookCopies.Add(copy);
                }
            }
            else
            {
                borrowing.BookCopies.Add(CreateBookCopy());
            }

            return borrowing;
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithNullUnitOfWork_ShouldThrow()
        {
            // Act & Assert
            Action act = () => new BorrowingService(null, _validator, _borrowingHelperServiceMock.Object, _loggerMock.Object, _configServiceMock.Object);
            act.Should().Throw<ArgumentNullException>().WithMessage("*unitOfWork*");
        }

        [TestMethod]
        public void Constructor_WithNullValidator_ShouldThrow()
        {
            // Act & Assert
            Action act = () => new BorrowingService(_unitOfWorkMock.Object, null, _borrowingHelperServiceMock.Object, _loggerMock.Object, _configServiceMock.Object);
            act.Should().Throw<ArgumentNullException>().WithMessage("*validator*");
        }

        [TestMethod]
        public void Constructor_WithNullBorrowingHelperService_ShouldThrow()
        {
            // Act & Assert
            Action act = () => new BorrowingService(_unitOfWorkMock.Object, _validator, null, _loggerMock.Object, _configServiceMock.Object);
            act.Should().Throw<ArgumentNullException>().WithMessage("*borrowingHelperService*");
        }

        [TestMethod]
        public void Constructor_WithNullLogger_ShouldThrow()
        {
            // Act & Assert
            Action act = () => new BorrowingService(_unitOfWorkMock.Object, _validator, _borrowingHelperServiceMock.Object, null, _configServiceMock.Object);
            act.Should().Throw<ArgumentNullException>().WithMessage("*logger*");
        }

        [TestMethod]
        public void Constructor_WithNullConfigService_ShouldThrow()
        {
            // Act & Assert
            Action act = () => new BorrowingService(_unitOfWorkMock.Object, _validator, _borrowingHelperServiceMock.Object, _loggerMock.Object, null);
            act.Should().Throw<ArgumentNullException>().WithMessage("*configService*");
        }

        #endregion

        #region CreateAsync Tests

        [TestMethod]
        public async Task CreateAsync_WithValidBorrowing_ShouldReturnSuccess()
        {
            // Arrange
            var borrowing = CreateValidBorrowing();
            var addedBorrowing = _fixture.Build<Borrowing>()
                .With(b => b.Id, borrowing.Id)
                .Create();

            _readerRepositoryMock.Setup(r => r.ExistsAsync(borrowing.Reader.Id)).ReturnsAsync(true);
            _librarianRepositoryMock.Setup(r => r.ExistsAsync(borrowing.Librarian.Id)).ReturnsAsync(true);

            foreach (var copy in borrowing.BookCopies)
            {
                _bookCopyRepositoryMock.Setup(r => r.GetByIdAsync(copy.Id)).ReturnsAsync(copy);
            }

            _configServiceMock.Setup(c => c.GetBorrowingPeriodDaysAsync()).ReturnsAsync(30);
            _borrowingRepositoryMock.Setup(r => r.AddAsync(borrowing)).ReturnsAsync(addedBorrowing);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(borrowing);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be(addedBorrowing);
        }

        [TestMethod]
        public async Task CreateAsync_WhenReaderNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var borrowing = CreateValidBorrowing();
            _readerRepositoryMock.Setup(r => r.ExistsAsync(borrowing.Reader.Id)).ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(borrowing);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Reader");
        }

        [TestMethod]
        public async Task CreateAsync_WhenLibrarianNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var borrowing = CreateValidBorrowing();
            _readerRepositoryMock.Setup(r => r.ExistsAsync(borrowing.Reader.Id)).ReturnsAsync(true);
            _librarianRepositoryMock.Setup(r => r.ExistsAsync(borrowing.Librarian.Id)).ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(borrowing);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Librarian");
        }

        [TestMethod]
        public async Task CreateAsync_WhenBookCopyNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var borrowing = CreateValidBorrowing();
            _readerRepositoryMock.Setup(r => r.ExistsAsync(borrowing.Reader.Id)).ReturnsAsync(true);
            _librarianRepositoryMock.Setup(r => r.ExistsAsync(borrowing.Librarian.Id)).ReturnsAsync(true);
            _bookCopyRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((BookCopy)null);

            // Act
            var result = await _service.CreateAsync(borrowing);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("BookCopy");
        }

        [TestMethod]
        public async Task CreateAsync_WhenBookCopyNotBorrowable_ShouldReturnBusinessRuleError()
        {
            // Arrange
            var bookCopy = CreateBookCopy();
            bookCopy.MarkAsBorrowed();
            var borrowing = CreateValidBorrowing(bookCopies: new List<BookCopy> { bookCopy });

            _readerRepositoryMock.Setup(r => r.ExistsAsync(borrowing.Reader.Id)).ReturnsAsync(true);
            _librarianRepositoryMock.Setup(r => r.ExistsAsync(borrowing.Librarian.Id)).ReturnsAsync(true);
            _bookCopyRepositoryMock.Setup(r => r.GetByIdAsync(bookCopy.Id)).ReturnsAsync(bookCopy);

            // Act
            var result = await _service.CreateAsync(borrowing);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("not available for borrowing");
        }

        [TestMethod]
        public async Task CreateAsync_WhenValidationFails_ShouldReturnValidationErrors()
        {
            // Arrange
            var borrowing = CreateValidBorrowing();
            borrowing.DueDate = borrowing.BorrowDate.AddDays(-1);

            // Act
            var result = await _service.CreateAsync(borrowing);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain("Due date must be after borrow date");
        }

        [TestMethod]
        public async Task CreateAsync_ShouldCallAllHelperServiceValidations()
        {
            // Arrange
            var borrowing = CreateValidBorrowing();

            _readerRepositoryMock.Setup(r => r.ExistsAsync(borrowing.Reader.Id)).ReturnsAsync(true);
            _librarianRepositoryMock.Setup(r => r.ExistsAsync(borrowing.Librarian.Id)).ReturnsAsync(true);

            foreach (var copy in borrowing.BookCopies)
            {
                _bookCopyRepositoryMock.Setup(r => r.GetByIdAsync(copy.Id)).ReturnsAsync(copy);
            }

            _configServiceMock.Setup(c => c.GetBorrowingPeriodDaysAsync()).ReturnsAsync(30);
            _borrowingRepositoryMock.Setup(r => r.AddAsync(borrowing)).ReturnsAsync(borrowing);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _service.CreateAsync(borrowing);

            // Assert
            _borrowingHelperServiceMock.Verify(h => h.ValidateMaxBooksPerBorrowingAsync(borrowing), Times.Once);
            _borrowingHelperServiceMock.Verify(h => h.ValidateMaxBooksInPeriodAsync(borrowing), Times.Once);
            _borrowingHelperServiceMock.Verify(h => h.ValidateMaxBooksSameDomainAsync(borrowing), Times.Once);
            _borrowingHelperServiceMock.Verify(h => h.ValidateMaxOvertimeSumDaysAsync(borrowing), Times.Once);
            _borrowingHelperServiceMock.Verify(h => h.ValidateSameBookDelayAsync(borrowing), Times.Once);
            _borrowingHelperServiceMock.Verify(h => h.ValidateMaxBooksPerDayAsync(borrowing), Times.Once);
            _borrowingHelperServiceMock.Verify(h => h.ValidateLibrarianLendingLimitAsync(borrowing), Times.Once);
        }

        #endregion

        #region GetByIdAsync Tests

        [TestMethod]
        public async Task GetByIdAsync_WhenBorrowingExists_ShouldReturnBorrowing()
        {
            // Arrange
            var borrowingId = Guid.NewGuid();
            var borrowing = _fixture.Build<Borrowing>().With(b => b.Id, borrowingId).Create();
            _borrowingRepositoryMock.Setup(r => r.GetByIdAsync(borrowingId)).ReturnsAsync(borrowing);

            // Act
            var result = await _service.GetByIdAsync(borrowingId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be(borrowing);
        }

        [TestMethod]
        public async Task GetByIdAsync_WhenBorrowingNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var borrowingId = Guid.NewGuid();
            _borrowingRepositoryMock.Setup(r => r.GetByIdAsync(borrowingId)).ReturnsAsync((Borrowing)null);

            // Act
            var result = await _service.GetByIdAsync(borrowingId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Borrowing with ID '{borrowingId}' not found");
        }

        #endregion

        #region GetAllAsync Tests

        [TestMethod]
        public async Task GetAllAsync_WhenBorrowingsExist_ShouldReturnAllBorrowings()
        {
            // Arrange
            var borrowings = _fixture.CreateMany<Borrowing>(3).ToList();
            _borrowingRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(borrowings);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(borrowings);
        }

        [TestMethod]
        public async Task GetAllAsync_WhenNoBorrowings_ShouldReturnEmptyList()
        {
            // Arrange
            _borrowingRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Borrowing>());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region UpdateAsync Tests

        [TestMethod]
        public async Task UpdateAsync_WithValidBorrowing_ShouldReturnSuccess()
        {
            // Arrange
            var borrowing = CreateValidBorrowing();
            _borrowingRepositoryMock.Setup(r => r.ExistsAsync(borrowing.Id)).ReturnsAsync(true);
            _borrowingRepositoryMock.Setup(r => r.UpdateAsync(borrowing)).ReturnsAsync(borrowing);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.UpdateAsync(borrowing);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [TestMethod]
        public async Task UpdateAsync_WhenBorrowingNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var borrowing = CreateValidBorrowing();
            _borrowingRepositoryMock.Setup(r => r.ExistsAsync(borrowing.Id)).ReturnsAsync(false);

            // Act
            var result = await _service.UpdateAsync(borrowing);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Borrowing with ID '{borrowing.Id}' not found");
        }

        [TestMethod]
        public async Task UpdateAsync_WhenValidationFails_ShouldReturnValidationErrors()
        {
            // Arrange
            var borrowing = CreateValidBorrowing();
            borrowing.DueDate = borrowing.BorrowDate.AddDays(-1);
            _borrowingRepositoryMock.Setup(r => r.ExistsAsync(borrowing.Id)).ReturnsAsync(true);

            // Act
            var result = await _service.UpdateAsync(borrowing);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain("Due date must be after borrow date");
        }

        #endregion

        #region DeleteAsync Tests

        [TestMethod]
        public async Task DeleteAsync_WhenBorrowingExistsAndNotFinished_ShouldReturnSuccess()
        {
            // Arrange
            var borrowingId = Guid.NewGuid();
            var borrowing = CreateValidBorrowing();
            borrowing.Id = borrowingId;
            borrowing.ReturnDate = null;

            _borrowingRepositoryMock.Setup(r => r.GetByIdAsync(borrowingId)).ReturnsAsync(borrowing);
            _borrowingRepositoryMock.Setup(r => r.DeleteAsync(borrowingId)).ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.DeleteAsync(borrowingId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [TestMethod]
        public async Task DeleteAsync_WhenBorrowingNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var borrowingId = Guid.NewGuid();
            _borrowingRepositoryMock.Setup(r => r.GetByIdAsync(borrowingId)).ReturnsAsync((Borrowing)null);

            // Act
            var result = await _service.DeleteAsync(borrowingId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Borrowing with ID '{borrowingId}' not found");
        }

        [TestMethod]
        public async Task DeleteAsync_WhenBorrowingIsFinished_ShouldReturnBusinessRuleError()
        {
            // Arrange
            var borrowingId = Guid.NewGuid();
            var borrowing = CreateValidBorrowing();
            borrowing.Id = borrowingId;
            borrowing.ReturnDate = DateTime.Now.Date;

            _borrowingRepositoryMock.Setup(r => r.GetByIdAsync(borrowingId)).ReturnsAsync(borrowing);

            // Act
            var result = await _service.DeleteAsync(borrowingId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Cannot delete a finished borrowing");
        }

        [TestMethod]
        public async Task DeleteAsync_WhenDeleteFails_ShouldReturnFailure()
        {
            // Arrange
            var borrowingId = Guid.NewGuid();
            var borrowing = CreateValidBorrowing();
            borrowing.Id = borrowingId;
            borrowing.ReturnDate = null;

            _borrowingRepositoryMock.Setup(r => r.GetByIdAsync(borrowingId)).ReturnsAsync(borrowing);
            _borrowingRepositoryMock.Setup(r => r.DeleteAsync(borrowingId)).ReturnsAsync(false);

            // Act
            var result = await _service.DeleteAsync(borrowingId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Failed to delete borrowing");
        }

        #endregion

        #region ExistsAsync Tests

        [TestMethod]
        public async Task ExistsAsync_WhenBorrowingExists_ShouldReturnTrue()
        {
            // Arrange
            var borrowingId = Guid.NewGuid();
            _borrowingRepositoryMock.Setup(r => r.ExistsAsync(borrowingId)).ReturnsAsync(true);

            // Act
            var result = await _service.ExistsAsync(borrowingId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [TestMethod]
        public async Task ExistsAsync_WhenBorrowingDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var borrowingId = Guid.NewGuid();
            _borrowingRepositoryMock.Setup(r => r.ExistsAsync(borrowingId)).ReturnsAsync(false);

            // Act
            var result = await _service.ExistsAsync(borrowingId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeFalse();
        }

        #endregion

        #region FinishBorrowingAsync Tests

        [TestMethod]
        public async Task FinishBorrowingAsync_WithValidBorrowing_ShouldReturnSuccess()
        {
            // Arrange
            var borrowingId = Guid.NewGuid();
            var borrowing = CreateValidBorrowing();
            borrowing.Id = borrowingId;
            borrowing.ReturnDate = null;

            _borrowingRepositoryMock.Setup(r => r.GetByIdAsync(borrowingId)).ReturnsAsync(borrowing);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.FinishBorrowingAsync(borrowingId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
            borrowing.ReturnDate.Should().NotBeNull();
        }

        [TestMethod]
        public async Task FinishBorrowingAsync_WithCustomReturnDate_ShouldUseProvidedDate()
        {
            // Arrange
            var borrowingId = Guid.NewGuid();
            var customReturnDate = DateTime.Now.Date.AddDays(-1);
            var borrowing = CreateValidBorrowing();
            borrowing.Id = borrowingId;
            borrowing.ReturnDate = null;

            _borrowingRepositoryMock.Setup(r => r.GetByIdAsync(borrowingId)).ReturnsAsync(borrowing);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.FinishBorrowingAsync(borrowingId, customReturnDate);

            // Assert
            result.Success.Should().BeTrue();
            borrowing.ReturnDate.Should().Be(customReturnDate);
        }

        [TestMethod]
        public async Task FinishBorrowingAsync_WhenBorrowingNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var borrowingId = Guid.NewGuid();
            _borrowingRepositoryMock.Setup(r => r.GetByIdAsync(borrowingId)).ReturnsAsync((Borrowing)null);

            // Act
            var result = await _service.FinishBorrowingAsync(borrowingId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Borrowing with ID '{borrowingId}' not found");
        }

        #endregion

        #region ExtendBorrowingAsync Tests

        [TestMethod]
        public async Task ExtendBorrowingAsync_WithValidExtension_ShouldReturnSuccess()
        {
            // Arrange
            var borrowingId = Guid.NewGuid();
            var borrowing = CreateValidBorrowing();
            borrowing.Id = borrowingId;
            borrowing.ReturnDate = null;
            borrowing.ExtensionDays = null;
            var originalDueDate = borrowing.DueDate;

            _borrowingRepositoryMock.Setup(r => r.GetByIdAsync(borrowingId)).ReturnsAsync(borrowing);
            _borrowingHelperServiceMock.Setup(h => h.ValidateMaxOvertimeSumDaysAsync(It.IsAny<Borrowing>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.ExtendBorrowingAsync(borrowingId, 7);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
            borrowing.ExtensionDays.Should().Be(7);
            borrowing.DueDate.Should().Be(originalDueDate.AddDays(7));
        }

        [TestMethod]
        public async Task ExtendBorrowingAsync_WhenAddingToExistingExtension_ShouldAccumulateDays()
        {
            // Arrange
            var borrowingId = Guid.NewGuid();
            var borrowing = CreateValidBorrowing();
            borrowing.Id = borrowingId;
            borrowing.ReturnDate = null;
            borrowing.ExtensionDays = 5;
            var originalDueDate = borrowing.DueDate;

            _borrowingRepositoryMock.Setup(r => r.GetByIdAsync(borrowingId)).ReturnsAsync(borrowing);
            _borrowingHelperServiceMock.Setup(h => h.ValidateMaxOvertimeSumDaysAsync(It.IsAny<Borrowing>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.ExtendBorrowingAsync(borrowingId, 3);

            // Assert
            result.Success.Should().BeTrue();
            borrowing.ExtensionDays.Should().Be(8);
            borrowing.DueDate.Should().Be(originalDueDate.AddDays(3));
        }

        [TestMethod]
        public async Task ExtendBorrowingAsync_WhenBorrowingNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var borrowingId = Guid.NewGuid();
            _borrowingRepositoryMock.Setup(r => r.GetByIdAsync(borrowingId)).ReturnsAsync((Borrowing)null);

            // Act
            var result = await _service.ExtendBorrowingAsync(borrowingId, 7);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Borrowing with ID '{borrowingId}' not found");
        }

        [TestMethod]
        public async Task ExtendBorrowingAsync_WhenBorrowingIsFinished_ShouldReturnBusinessRuleError()
        {
            // Arrange
            var borrowingId = Guid.NewGuid();
            var borrowing = CreateValidBorrowing();
            borrowing.Id = borrowingId;
            borrowing.ReturnDate = DateTime.Now.Date;

            _borrowingRepositoryMock.Setup(r => r.GetByIdAsync(borrowingId)).ReturnsAsync(borrowing);

            // Act
            var result = await _service.ExtendBorrowingAsync(borrowingId, 7);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Cannot extend a finished borrowing");
        }

        [TestMethod]
        public async Task ExtendBorrowingAsync_WithNonPositiveExtensionDays_ShouldReturnValidationError()
        {
            // Arrange
            var borrowingId = Guid.NewGuid();
            var borrowing = CreateValidBorrowing();
            borrowing.Id = borrowingId;
            borrowing.ReturnDate = null;

            _borrowingRepositoryMock.Setup(r => r.GetByIdAsync(borrowingId)).ReturnsAsync(borrowing);

            // Act
            var result = await _service.ExtendBorrowingAsync(borrowingId, 0);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain("Extension days must be positive");
        }

        [TestMethod]
        public async Task ExtendBorrowingAsync_WhenHelperValidationFails_ShouldReturnBusinessRuleError()
        {
            // Arrange
            var borrowingId = Guid.NewGuid();
            var borrowing = CreateValidBorrowing();
            borrowing.Id = borrowingId;
            borrowing.ReturnDate = null;

            _borrowingRepositoryMock.Setup(r => r.GetByIdAsync(borrowingId)).ReturnsAsync(borrowing);
            _borrowingHelperServiceMock.Setup(h => h.ValidateMaxOvertimeSumDaysAsync(It.IsAny<Borrowing>()))
                .ThrowsAsync(new AggregateValidationException("Exceeds overtime limit"));

            // Act
            var result = await _service.ExtendBorrowingAsync(borrowingId, 7);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain("Exceeds overtime limit");
        }

        #endregion
    }
}
