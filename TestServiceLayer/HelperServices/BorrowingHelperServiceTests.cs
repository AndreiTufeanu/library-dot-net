using AutoFixture;
using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLayer.Exceptions;
using ServiceLayer.ServiceContracts;
using ServiceLayer.ServiceContracts.HelperServiceContracts;
using ServiceLayer.Services.HelperServices;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServiceLayer.HelperServices
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class BorrowingHelperServiceTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IConfigurationSettingService> _configServiceMock;
        private readonly Mock<IBookHelperService> _bookHelperServiceMock;
        private readonly BorrowingHelperService _service;

        private readonly Mock<ILibrarianRepository> _librarianRepositoryMock;
        private readonly Mock<IBookRepository> _bookRepositoryMock;
        private readonly Mock<IBorrowingRepository> _borrowingRepositoryMock;

        public BorrowingHelperServiceTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _configServiceMock = new Mock<IConfigurationSettingService>();
            _bookHelperServiceMock = new Mock<IBookHelperService>();

            _librarianRepositoryMock = new Mock<ILibrarianRepository>();
            _borrowingRepositoryMock = new Mock<IBorrowingRepository>();
            _bookRepositoryMock = new Mock<IBookRepository>();

            _unitOfWorkMock.Setup(u => u.Librarians).Returns(_librarianRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.Borrowings).Returns(_borrowingRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.Books).Returns(_bookRepositoryMock.Object);

            _service = new BorrowingHelperService(_unitOfWorkMock.Object, _configServiceMock.Object, _bookHelperServiceMock.Object);
        }

        private BookCopy CreateBookCopy(bool isAvailable = true, bool isLectureRoomOnly = false)
        {
            return _fixture.Build<BookCopy>()
                .FromFactory(() => new BookCopy(isLectureRoomOnly: isLectureRoomOnly))
                .Create();
        }

        private Book CreateBook(Guid? id = null)
        {
            return _fixture.Build<Book>()
                .FromFactory(() => new Book(initialCopies: 10))
                .With(b => b.Id, id ?? Guid.NewGuid())
                .Create();
        }

        private Reader CreateReader(Guid? id = null)
        {
            return _fixture.Build<Reader>()
                .With(r => r.Id, id ?? Guid.NewGuid())
                .Create();
        }

        private Librarian CreateLibrarian(Guid? id = null, Reader reader = null)
        {
            return _fixture.Build<Librarian>()
                .With(l => l.Id, id ?? Guid.NewGuid())
                .With(l => l.ReaderDetails, reader)
                .Create();
        }

        private Borrowing CreateBorrowing(Reader reader = null, Librarian librarian = null, List<BookCopy> bookCopies = null)
        {
            var borrowing = _fixture.Build<Borrowing>()
                .With(b => b.BorrowDate, DateTime.Now.Date)
                .With(b => b.DueDate, DateTime.Now.Date.AddDays(30))
                .With(b => b.ReturnDate, (DateTime?)null)
                .Without(b => b.ExtensionDays)
                .Create();

            if (reader != null)
                borrowing.Reader = reader;

            if (librarian != null)
                borrowing.Librarian = librarian;

            if (bookCopies != null)
            {
                foreach (var copy in bookCopies)
                {
                    borrowing.BookCopies.Add(copy);
                }
            }

            return borrowing;
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithNullUnitOfWork_ShouldThrow()
        {
            // Act & Assert
            Action act = () => new BorrowingHelperService(null, Mock.Of<IConfigurationSettingService>(), Mock.Of<IBookHelperService>());
            act.Should().Throw<ArgumentNullException>().WithMessage("*unitOfWork*");
        }

        [TestMethod]
        public void Constructor_WithNullConfigService_ShouldThrow()
        {
            // Act & Assert
            Action act = () => new BorrowingHelperService(Mock.Of<IUnitOfWork>(), null, Mock.Of<IBookHelperService>());
            act.Should().Throw<ArgumentNullException>().WithMessage("*configService*");
        }

        [TestMethod]
        public void Constructor_WithNullBookHelperService_ShouldThrow()
        {
            // Act & Assert
            Action act = () => new BorrowingHelperService(Mock.Of<IUnitOfWork>(), Mock.Of<IConfigurationSettingService>(), null);
            act.Should().Throw<ArgumentNullException>().WithMessage("*bookHelperService*");
        }

        #endregion

        #region IsReaderAlsoLibrarianAsync Tests

        [TestMethod]
        public async Task IsReaderAlsoLibrarianAsync_WhenReaderIsLibrarian_ShouldReturnTrue()
        {
            // Arrange
            var readerId = Guid.NewGuid();
            _librarianRepositoryMock.Setup(r => r.IsReaderAlsoLibrarianAsync(readerId)).ReturnsAsync(true);

            // Act
            var result = await _service.IsReaderAlsoLibrarianAsync(readerId);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public async Task IsReaderAlsoLibrarianAsync_WhenReaderIsNotLibrarian_ShouldReturnFalse()
        {
            // Arrange
            var readerId = Guid.NewGuid();
            _librarianRepositoryMock.Setup(r => r.IsReaderAlsoLibrarianAsync(readerId)).ReturnsAsync(false);

            // Act
            var result = await _service.IsReaderAlsoLibrarianAsync(readerId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region ValidateMaxBooksPerBorrowingAsync Tests

        [TestMethod]
        public async Task ValidateMaxBooksPerBorrowingAsync_WithValidBookCount_ShouldNotThrow()
        {
            // Arrange
            var reader = CreateReader();
            var librarian = CreateLibrarian();
            var bookCopies = _fixture.CreateMany<BookCopy>(3).ToList();
            var borrowing = CreateBorrowing(reader, librarian, bookCopies);

            _librarianRepositoryMock.Setup(r => r.IsReaderAlsoLibrarianAsync(reader.Id)).ReturnsAsync(false);
            _configServiceMock.Setup(c => c.GetMaxBooksPerBorrowingAsync(false)).ReturnsAsync(5);

            // Act
            Func<Task> act = async () => await _service.ValidateMaxBooksPerBorrowingAsync(borrowing);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task ValidateMaxBooksPerBorrowingAsync_WhenReaderIsLibrarian_ShouldUseHigherLimit()
        {
            // Arrange
            var reader = CreateReader();
            var librarian = CreateLibrarian(reader: reader);
            var bookCopies = _fixture.CreateMany<BookCopy>(7).ToList();
            var borrowing = CreateBorrowing(reader, librarian, bookCopies);

            _librarianRepositoryMock.Setup(r => r.IsReaderAlsoLibrarianAsync(reader.Id)).ReturnsAsync(true);
            _configServiceMock.Setup(c => c.GetMaxBooksPerBorrowingAsync(true)).ReturnsAsync(10);

            // Act
            Func<Task> act = async () => await _service.ValidateMaxBooksPerBorrowingAsync(borrowing);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task ValidateMaxBooksPerBorrowingAsync_WhenExceedsLimit_ShouldThrow()
        {
            // Arrange
            var reader = CreateReader();
            var librarian = CreateLibrarian();
            var bookCopies = _fixture.CreateMany<BookCopy>(6).ToList();
            var borrowing = CreateBorrowing(reader, librarian, bookCopies);

            _librarianRepositoryMock.Setup(r => r.IsReaderAlsoLibrarianAsync(reader.Id)).ReturnsAsync(false);
            _configServiceMock.Setup(c => c.GetMaxBooksPerBorrowingAsync(false)).ReturnsAsync(5);

            // Act
            Func<Task> act = async () => await _service.ValidateMaxBooksPerBorrowingAsync(borrowing);

            // Assert
            await act.Should().ThrowAsync<AggregateValidationException>();
        }

        [TestMethod]
        public async Task ValidateMaxBooksPerBorrowingAsync_WithDuplicateBookTitles_ShouldCountDistinctBooks()
        {
            // Arrange
            var reader = CreateReader();
            var librarian = CreateLibrarian();
            var book = CreateBook();

            var bookCopy1 = CreateBookCopy();
            var bookCopy2 = CreateBookCopy();
            bookCopy1.Edition = _fixture.Build<Edition>().With(e => e.Book, book).Create();
            bookCopy2.Edition = _fixture.Build<Edition>().With(e => e.Book, book).Create();

            var borrowing = CreateBorrowing(reader, librarian, new List<BookCopy> { bookCopy1, bookCopy2 });

            _librarianRepositoryMock.Setup(r => r.IsReaderAlsoLibrarianAsync(reader.Id)).ReturnsAsync(false);
            _configServiceMock.Setup(c => c.GetMaxBooksPerBorrowingAsync(false)).ReturnsAsync(5);

            // Act
            Func<Task> act = async () => await _service.ValidateMaxBooksPerBorrowingAsync(borrowing);

            // Assert
            await act.Should().NotThrowAsync();
        }

        #endregion

        #region ValidateMaxBooksInPeriodAsync Tests

        [TestMethod]
        public async Task ValidateMaxBooksInPeriodAsync_WithValidBorrowingCount_ShouldNotThrow()
        {
            // Arrange
            var reader = CreateReader();
            var librarian = CreateLibrarian();
            var borrowing = CreateBorrowing(reader, librarian);

            _librarianRepositoryMock.Setup(r => r.IsReaderAlsoLibrarianAsync(reader.Id)).ReturnsAsync(false);
            _configServiceMock.Setup(c => c.GetMaxBooksInPeriodAsync(false)).ReturnsAsync(10);
            _configServiceMock.Setup(c => c.GetMaxBooksInPeriodWindowDaysAsync(false)).ReturnsAsync(14);
            _borrowingRepositoryMock.Setup(r => r.GetCountByReaderInPeriodAsync(
                reader.Id, It.IsAny<DateTime>(), borrowing.BorrowDate)).ReturnsAsync(5);

            // Act
            Func<Task> act = async () => await _service.ValidateMaxBooksInPeriodAsync(borrowing);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task ValidateMaxBooksInPeriodAsync_WhenExceedsPeriodLimit_ShouldThrow()
        {
            // Arrange
            var reader = CreateReader();
            var librarian = CreateLibrarian();
            var borrowing = CreateBorrowing(reader, librarian);

            _librarianRepositoryMock.Setup(r => r.IsReaderAlsoLibrarianAsync(reader.Id)).ReturnsAsync(false);
            _configServiceMock.Setup(c => c.GetMaxBooksInPeriodAsync(false)).ReturnsAsync(10);
            _configServiceMock.Setup(c => c.GetMaxBooksInPeriodWindowDaysAsync(false)).ReturnsAsync(14);
            _borrowingRepositoryMock.Setup(r => r.GetCountByReaderInPeriodAsync(
                reader.Id, It.IsAny<DateTime>(), borrowing.BorrowDate)).ReturnsAsync(11);

            // Act
            Func<Task> act = async () => await _service.ValidateMaxBooksInPeriodAsync(borrowing);

            // Assert
            await act.Should().ThrowAsync<AggregateValidationException>();
        }

        [TestMethod]
        public async Task ValidateMaxBooksInPeriodAsync_WhenReaderIsLibrarian_ShouldUseHigherLimit()
        {
            // Arrange
            var reader = CreateReader();
            var librarian = CreateLibrarian(reader: reader);
            var borrowing = CreateBorrowing(reader, librarian);

            _librarianRepositoryMock.Setup(r => r.IsReaderAlsoLibrarianAsync(reader.Id)).ReturnsAsync(true);
            _configServiceMock.Setup(c => c.GetMaxBooksInPeriodAsync(true)).ReturnsAsync(20);
            _configServiceMock.Setup(c => c.GetMaxBooksInPeriodWindowDaysAsync(true)).ReturnsAsync(7);
            _borrowingRepositoryMock.Setup(r => r.GetCountByReaderInPeriodAsync(
                reader.Id, It.IsAny<DateTime>(), borrowing.BorrowDate)).ReturnsAsync(15);

            // Act
            Func<Task> act = async () => await _service.ValidateMaxBooksInPeriodAsync(borrowing);

            // Assert
            await act.Should().NotThrowAsync();
        }

        #endregion

        #region ValidateMaxBooksSameDomainAsync Tests

        [TestMethod]
        public async Task ValidateMaxBooksSameDomainAsync_WithValidDomainCount_ShouldNotThrow()
        {
            // Arrange
            var reader = CreateReader();
            var librarian = CreateLibrarian();
            var book = CreateBook();
            var bookCopy = CreateBookCopy();
            bookCopy.Edition = _fixture.Build<Edition>().With(e => e.Book, book).Create();
            var borrowing = CreateBorrowing(reader, librarian, new List<BookCopy> { bookCopy });

            var domainHierarchy = new HashSet<Guid> { Guid.NewGuid() };

            _librarianRepositoryMock.Setup(r => r.IsReaderAlsoLibrarianAsync(reader.Id)).ReturnsAsync(false);
            _configServiceMock.Setup(c => c.GetMaxBooksSameDomainAsync(false)).ReturnsAsync(3);
            _configServiceMock.Setup(c => c.GetSameDomainTimeLimitMonthsAsync()).ReturnsAsync(3);
            _bookHelperServiceMock.Setup(b => b.GetCompleteDomainHierarchyForBookAsync(book.Id)).ReturnsAsync(domainHierarchy);
            _borrowingRepositoryMock.Setup(r => r.GetCountByReaderAndDomainsInPeriodAsync(
                reader.Id, It.IsAny<List<Guid>>(), It.IsAny<DateTime>())).ReturnsAsync(2);

            // Act
            Func<Task> act = async () => await _service.ValidateMaxBooksSameDomainAsync(borrowing);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task ValidateMaxBooksSameDomainAsync_WhenExceedsDomainLimit_ShouldThrow()
        {
            // Arrange
            var reader = CreateReader();
            var librarian = CreateLibrarian();
            var book = CreateBook();
            var bookCopy = CreateBookCopy();
            bookCopy.Edition = _fixture.Build<Edition>().With(e => e.Book, book).Create();
            var borrowing = CreateBorrowing(reader, librarian, new List<BookCopy> { bookCopy });

            var domainHierarchy = new HashSet<Guid> { Guid.NewGuid() };

            _librarianRepositoryMock.Setup(r => r.IsReaderAlsoLibrarianAsync(reader.Id)).ReturnsAsync(false);
            _configServiceMock.Setup(c => c.GetMaxBooksSameDomainAsync(false)).ReturnsAsync(3);
            _configServiceMock.Setup(c => c.GetSameDomainTimeLimitMonthsAsync()).ReturnsAsync(3);
            _bookRepositoryMock.Setup(b => b.GetByIdAsync(book.Id)).ReturnsAsync(book);
            _bookHelperServiceMock.Setup(b => b.GetCompleteDomainHierarchyForBookAsync(book.Id)).ReturnsAsync(domainHierarchy);
            _borrowingRepositoryMock.Setup(r => r.GetCountByReaderAndDomainsInPeriodAsync(
                reader.Id, It.IsAny<List<Guid>>(), It.IsAny<DateTime>())).ReturnsAsync(4);

            // Act
            Func<Task> act = async () => await _service.ValidateMaxBooksSameDomainAsync(borrowing);

            // Assert
            await act.Should().ThrowAsync<AggregateValidationException>();
        }

        #endregion

        #region ValidateMaxOvertimeSumDaysAsync Tests

        [TestMethod]
        public async Task ValidateMaxOvertimeSumDaysAsync_WithoutExtensionDays_ShouldNotThrow()
        {
            // Arrange
            var reader = CreateReader();
            var librarian = CreateLibrarian();
            var borrowing = CreateBorrowing(reader, librarian);
            borrowing.ExtensionDays = null;

            // Act
            Func<Task> act = async () => await _service.ValidateMaxOvertimeSumDaysAsync(borrowing);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task ValidateMaxOvertimeSumDaysAsync_WithValidExtensionDays_ShouldNotThrow()
        {
            // Arrange
            var reader = CreateReader();
            var librarian = CreateLibrarian();
            var borrowing = CreateBorrowing(reader, librarian);
            borrowing.ExtensionDays = 5;

            _librarianRepositoryMock.Setup(r => r.IsReaderAlsoLibrarianAsync(reader.Id)).ReturnsAsync(false);
            _configServiceMock.Setup(c => c.GetMaxOvertimeSumDaysAsync(false)).ReturnsAsync(14);
            _configServiceMock.Setup(c => c.GetExtensionWindowMonthsAsync()).ReturnsAsync(3);
            _borrowingRepositoryMock.Setup(r => r.GetTotalExtensionDaysByReaderInPeriodAsync(
                reader.Id, It.IsAny<DateTime>())).ReturnsAsync(7);

            // Act
            Func<Task> act = async () => await _service.ValidateMaxOvertimeSumDaysAsync(borrowing);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task ValidateMaxOvertimeSumDaysAsync_WhenExceedsOvertimeLimit_ShouldThrow()
        {
            // Arrange
            var reader = CreateReader();
            var librarian = CreateLibrarian();
            var borrowing = CreateBorrowing(reader, librarian);
            borrowing.ExtensionDays = 10;

            _librarianRepositoryMock.Setup(r => r.IsReaderAlsoLibrarianAsync(reader.Id)).ReturnsAsync(false);
            _configServiceMock.Setup(c => c.GetMaxOvertimeSumDaysAsync(false)).ReturnsAsync(14);
            _configServiceMock.Setup(c => c.GetExtensionWindowMonthsAsync()).ReturnsAsync(3);
            _borrowingRepositoryMock.Setup(r => r.GetTotalExtensionDaysByReaderInPeriodAsync(
                reader.Id, It.IsAny<DateTime>())).ReturnsAsync(10);

            // Act
            Func<Task> act = async () => await _service.ValidateMaxOvertimeSumDaysAsync(borrowing);

            // Assert
            await act.Should().ThrowAsync<AggregateValidationException>();
        }

        #endregion

        #region ValidateSameBookDelayAsync Tests

        [TestMethod]
        public async Task ValidateSameBookDelayAsync_WhenBookNeverBorrowed_ShouldNotThrow()
        {
            // Arrange
            var reader = CreateReader();
            var librarian = CreateLibrarian();
            var book = CreateBook();
            var bookCopy = CreateBookCopy();
            bookCopy.Edition = _fixture.Build<Edition>().With(e => e.Book, book).Create();
            var borrowing = CreateBorrowing(reader, librarian, new List<BookCopy> { bookCopy });

            _librarianRepositoryMock.Setup(r => r.IsReaderAlsoLibrarianAsync(reader.Id)).ReturnsAsync(false);
            _configServiceMock.Setup(c => c.GetSameBookDelayDaysAsync(false)).ReturnsAsync(7);
            _borrowingRepositoryMock.Setup(r => r.GetLastBorrowDateForBookByReaderAsync(
                reader.Id, book.Id)).ReturnsAsync((DateTime?)null);

            // Act
            Func<Task> act = async () => await _service.ValidateSameBookDelayAsync(borrowing);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task ValidateSameBookDelayAsync_WhenDelayPeriodHasPassed_ShouldNotThrow()
        {
            // Arrange
            var reader = CreateReader();
            var librarian = CreateLibrarian();
            var book = CreateBook();
            var bookCopy = CreateBookCopy();
            bookCopy.Edition = _fixture.Build<Edition>().With(e => e.Book, book).Create();
            var borrowing = CreateBorrowing(reader, librarian, new List<BookCopy> { bookCopy });
            borrowing.BorrowDate = DateTime.Now.Date;

            _librarianRepositoryMock.Setup(r => r.IsReaderAlsoLibrarianAsync(reader.Id)).ReturnsAsync(false);
            _configServiceMock.Setup(c => c.GetSameBookDelayDaysAsync(false)).ReturnsAsync(7);
            _borrowingRepositoryMock.Setup(r => r.GetLastBorrowDateForBookByReaderAsync(
                reader.Id, book.Id)).ReturnsAsync(DateTime.Now.Date.AddDays(-10));

            // Act
            Func<Task> act = async () => await _service.ValidateSameBookDelayAsync(borrowing);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task ValidateSameBookDelayAsync_WhenDelayPeriodNotPassed_ShouldThrow()
        {
            // Arrange
            var reader = CreateReader();
            var librarian = CreateLibrarian();
            var book = CreateBook();
            var bookCopy = CreateBookCopy();
            bookCopy.Edition = _fixture.Build<Edition>().With(e => e.Book, book).Create();
            var borrowing = CreateBorrowing(reader, librarian, new List<BookCopy> { bookCopy });
            borrowing.BorrowDate = DateTime.Now.Date;

            _librarianRepositoryMock.Setup(r => r.IsReaderAlsoLibrarianAsync(reader.Id)).ReturnsAsync(false);
            _configServiceMock.Setup(c => c.GetSameBookDelayDaysAsync(false)).ReturnsAsync(7);
            _borrowingRepositoryMock.Setup(r => r.GetLastBorrowDateForBookByReaderAsync(
                reader.Id, book.Id)).ReturnsAsync(DateTime.Now.Date.AddDays(-5));

            // Act
            Func<Task> act = async () => await _service.ValidateSameBookDelayAsync(borrowing);

            // Assert
            await act.Should().ThrowAsync<AggregateValidationException>();
        }

        #endregion

        #region ValidateMaxBooksPerDayAsync Tests

        [TestMethod]
        public async Task ValidateMaxBooksPerDayAsync_WhenReaderIsLibrarian_ShouldSkipValidation()
        {
            // Arrange
            var reader = CreateReader();
            var librarian = CreateLibrarian(reader: reader);
            var borrowing = CreateBorrowing(reader, librarian);

            _librarianRepositoryMock.Setup(r => r.IsReaderAlsoLibrarianAsync(reader.Id)).ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _service.ValidateMaxBooksPerDayAsync(borrowing);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task ValidateMaxBooksPerDayAsync_WithValidDailyCount_ShouldNotThrow()
        {
            // Arrange
            var reader = CreateReader();
            var librarian = CreateLibrarian();
            var borrowing = CreateBorrowing(reader, librarian);

            _librarianRepositoryMock.Setup(r => r.IsReaderAlsoLibrarianAsync(reader.Id)).ReturnsAsync(false);
            _configServiceMock.Setup(c => c.GetMaxBooksPerDayAsync()).ReturnsAsync(10);
            _borrowingRepositoryMock.Setup(r => r.GetCountByReaderOnDateAsync(
                reader.Id, borrowing.BorrowDate.Date)).ReturnsAsync(5);

            // Act
            Func<Task> act = async () => await _service.ValidateMaxBooksPerDayAsync(borrowing);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task ValidateMaxBooksPerDayAsync_WhenExceedsDailyLimit_ShouldThrow()
        {
            // Arrange
            var reader = CreateReader();
            var librarian = CreateLibrarian();
            var borrowing = CreateBorrowing(reader, librarian);

            _librarianRepositoryMock.Setup(r => r.IsReaderAlsoLibrarianAsync(reader.Id)).ReturnsAsync(false);
            _configServiceMock.Setup(c => c.GetMaxBooksPerDayAsync()).ReturnsAsync(10);
            _borrowingRepositoryMock.Setup(r => r.GetCountByReaderOnDateAsync(
                reader.Id, borrowing.BorrowDate.Date)).ReturnsAsync(11);

            // Act
            Func<Task> act = async () => await _service.ValidateMaxBooksPerDayAsync(borrowing);

            // Assert
            await act.Should().ThrowAsync<AggregateValidationException>();
        }

        #endregion

        #region ValidateLibrarianLendingLimitAsync Tests

        [TestMethod]
        public async Task ValidateLibrarianLendingLimitAsync_WithValidLendingCount_ShouldNotThrow()
        {
            // Arrange
            var reader = CreateReader();
            var librarian = CreateLibrarian();
            var borrowing = CreateBorrowing(reader, librarian);

            _configServiceMock.Setup(c => c.GetMaxBooksLentPerDayAsync()).ReturnsAsync(50);
            _borrowingRepositoryMock.Setup(r => r.GetCountByLibrarianOnDateAsync(
                librarian.Id, borrowing.BorrowDate.Date)).ReturnsAsync(30);

            // Act
            Func<Task> act = async () => await _service.ValidateLibrarianLendingLimitAsync(borrowing);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task ValidateLibrarianLendingLimitAsync_WhenExceedsLendingLimit_ShouldThrow()
        {
            // Arrange
            var reader = CreateReader();
            var librarian = CreateLibrarian();
            var borrowing = CreateBorrowing(reader, librarian);

            _configServiceMock.Setup(c => c.GetMaxBooksLentPerDayAsync()).ReturnsAsync(50);
            _borrowingRepositoryMock.Setup(r => r.GetCountByLibrarianOnDateAsync(
                librarian.Id, borrowing.BorrowDate.Date)).ReturnsAsync(51);

            // Act
            Func<Task> act = async () => await _service.ValidateLibrarianLendingLimitAsync(borrowing);

            // Assert
            await act.Should().ThrowAsync<AggregateValidationException>();
        }

        #endregion
    }
}
