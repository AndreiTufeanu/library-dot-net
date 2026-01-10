using AutoFixture;
using DomainModel.Entities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLayer.Validators;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServiceLayer.Validators
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class BorrowingValidatorTests : ValidatorTestBase<BorrowingValidator, Borrowing>
    {
        private Func<string, string, BookCopy> _bookCopyFactory;

        [TestInitialize]
        public void TestInitialize_BorrowingValidatorTests()
        {
            var domainCache = new Dictionary<string, Domain>();

            _fixture.Register<Func<string, string, BookCopy>>(() => (title, domainName) =>
            {
                if (!domainCache.TryGetValue(domainName, out var domain))
                {
                    domain = new Domain { Id = Guid.NewGuid(), Name = domainName };
                    domainCache[domainName] = domain;
                }

                var book = new Book(initialCopies: 10)
                {
                    Id = Guid.NewGuid(),
                    Title = title,
                };
                book.Domains.Add(domain);

                var edition = new Edition
                {
                    Id = Guid.NewGuid(),
                    Book = book,
                    BookType = new BookType { Id = Guid.NewGuid(), Name = "Hardcover" },
                    NumberOfPages = 300,
                    PublicationDate = DateTime.Now,
                };

                return new BookCopy(isLectureRoomOnly: false) { Id = Guid.NewGuid(), Edition = edition };
            });

            _bookCopyFactory = _fixture.Create<Func<string, string, BookCopy>>();
        }

        [TestMethod]
        public void Validate_BorrowingWithValidData_ShouldPassValidation()
        {
            // Arrange
            var borrowing = _fixture.Build<Borrowing>()
                .With(b => b.BorrowDate, DateTime.Now.AddDays(-1))
                .With(b => b.DueDate, DateTime.Now.AddDays(30))
                .With(b => b.ReturnDate, (DateTime?)null)
                .With(b => b.ExtensionDays, (int?)null)
                .Create();

            var bookCopy1 = _bookCopyFactory("Book 1", "Science Fiction");
            var bookCopy2 = _bookCopyFactory("Book 2", "Fantasy");
            borrowing.BookCopies.Add(bookCopy1);
            borrowing.BookCopies.Add(bookCopy2);

            // Act
            var result = Validate(borrowing);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void Validate_BorrowingWithDueDateBeforeBorrowDate_ShouldFailValidation()
        {
            // Arrange
            var borrowing = _fixture.Build<Borrowing>()
                .With(b => b.BorrowDate, DateTime.Now)
                .With(b => b.DueDate, DateTime.Now.AddDays(-1))
                .Create();

            borrowing.BookCopies.Add(_bookCopyFactory("Test Book", "Test Domain"));

            // Act
            var result = Validate(borrowing);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("Due date must be after borrow date"));
        }

        [TestMethod]
        public void Validate_BorrowingWithReturnDateBeforeBorrowDate_ShouldFailValidation()
        {
            // Arrange
            var borrowing = _fixture.Build<Borrowing>()
                .With(b => b.BorrowDate, DateTime.Now)
                .With(b => b.DueDate, DateTime.Now.AddDays(30))
                .With(b => b.ReturnDate, DateTime.Now.AddDays(-1))
                .Create();
                
            borrowing.BookCopies.Add(_bookCopyFactory("Test Book", "Test Domain"));

            // Act
            var result = Validate(borrowing);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("Return date must be on or after borrow date"));
        }

        [TestMethod]
        public void Validate_BorrowingWithNegativeExtensionDays_ShouldFailValidation()
        {
            // Arrange
            var borrowing = _fixture.Build<Borrowing>()
                .With(b => b.BorrowDate, DateTime.Now.AddDays(-1))
                .With(b => b.DueDate, DateTime.Now.AddDays(30))
                .With(b => b.ExtensionDays, -5)
                .Create();

            borrowing.BookCopies.Add(_bookCopyFactory("Test Book", "Test Domain"));

            // Act
            var result = Validate(borrowing);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("Extension days cannot be negative"));
        }

        [TestMethod]
        public void Validate_BorrowingWithNoBookCopies_ShouldFailValidation()
        {
            // Arrange
            var borrowing = _fixture.Build<Borrowing>()
                .With(b => b.BorrowDate, DateTime.Now)
                .With(b => b.DueDate, DateTime.Now.AddDays(30))
                .Create();

            // Act
            var result = Validate(borrowing);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("At least one book copy must be borrowed"));
        }

        [TestMethod]
        public void Validate_BorrowingWithMultipleCopiesOfSameBook_ShouldFailValidation()
        {
            // Arrange
            var borrowing = _fixture.Build<Borrowing>()
                .With(b => b.BorrowDate, DateTime.Now)
                .With(b => b.DueDate, DateTime.Now.AddDays(30))
                .Create();

            var bookCopy1 = _bookCopyFactory("Same Book", "Test Domain");
            var bookCopy2 = _bookCopyFactory("Same Book", "Test Domain");

            bookCopy2.Edition.Book = bookCopy1.Edition.Book;

            borrowing.BookCopies.Add(bookCopy1);
            borrowing.BookCopies.Add(bookCopy2);

            // Act
            var result = Validate(borrowing);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("Cannot borrow multiple copies/editions of the same book title"));
        }

        [TestMethod]
        public void Validate_BorrowingThreeBooksFromSameDomain_ShouldFailDomainDiversityRule()
        {
            // Arrange
            var borrowing = _fixture.Build<Borrowing>()
                .With(b => b.BorrowDate, DateTime.Now)
                .With(b => b.DueDate, DateTime.Now.AddDays(30))
                .Without(b => b.ReturnDate)
                .Create();
            
            var bookCopy1 = _bookCopyFactory("Book 1", "Science Fiction");
            var bookCopy2 = _bookCopyFactory("Book 2", "Science Fiction");
            var bookCopy3 = _bookCopyFactory("Book 3", "Science Fiction");

            borrowing.BookCopies.Add(bookCopy1);
            borrowing.BookCopies.Add(bookCopy2);
            borrowing.BookCopies.Add(bookCopy3);

            // Act
            var result = Validate(borrowing);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("they must belong to at least 2 distinct domains"));
        }

        [TestMethod]
        public void Validate_BorrowingThreeBooksFromDifferentDomains_ShouldPassDomainDiversityRule()
        {
            // Arrange
            var borrowing = _fixture.Build<Borrowing>()
                .With(b => b.BorrowDate, DateTime.Now)
                .With(b => b.DueDate, DateTime.Now.AddDays(30))
                .Without(b => b.ReturnDate)
                .Create();

            var bookCopy1 = _bookCopyFactory("Book 1", "Science Fiction");
            var bookCopy2 = _bookCopyFactory("Book 2", "Fantasy");
            var bookCopy3 = _bookCopyFactory("Book 3", "Fantasy");

            borrowing.BookCopies.Add(bookCopy1);
            borrowing.BookCopies.Add(bookCopy2);
            borrowing.BookCopies.Add(bookCopy3);

            // Act
            var result = Validate(borrowing);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void Validate_BorrowingTwoBooks_ShouldNotCheckDomainDiversity()
        {
            // Arrange
            var borrowing = _fixture.Build<Borrowing>()
                .With(b => b.BorrowDate, DateTime.Now)
                .With(b => b.DueDate, DateTime.Now.AddDays(30))
                .Without(b => b.ReturnDate)
                .Create();

            var bookCopy1 = _bookCopyFactory("Book 1", "Science Fiction");
            var bookCopy2 = _bookCopyFactory("Book 2", "Science Fiction");

            borrowing.BookCopies.Add(bookCopy1);
            borrowing.BookCopies.Add(bookCopy2);

            // Act
            var result = Validate(borrowing);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void Validate_BorrowingWithZeroExtensionDays_ShouldPassValidation()
        {
            // Arrange
            var borrowing = _fixture.Build<Borrowing>()
                .With(b => b.BorrowDate, DateTime.Now)
                .With(b => b.DueDate, DateTime.Now.AddDays(30))
                .Without(b => b.ReturnDate)
                .With(b => b.ExtensionDays, 0)
                .Create();

            borrowing.BookCopies.Add(_bookCopyFactory("Test Book", "Test Domain"));

            // Act
            var result = Validate(borrowing);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void Validate_BorrowingWithBookCopyHavingNullEdition_ShouldFailValidation()
        {
            // Arrange
            var borrowing = _fixture.Build<Borrowing>()
                .With(b => b.BorrowDate, DateTime.Now)
                .With(b => b.DueDate, DateTime.Now.AddDays(30))
                .Without(b => b.ReturnDate)
                .Create();

            var validBookCopy = _bookCopyFactory("Valid Book", "Test Domain");
            var bookCopyWithNullEdition = new BookCopy(isLectureRoomOnly: false)
            {
                Id = Guid.NewGuid(),
                Edition = null
            };

            borrowing.BookCopies.Add(validBookCopy);
            borrowing.BookCopies.Add(bookCopyWithNullEdition);

            // Act
            var result = Validate(borrowing);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [TestMethod]
        public void Validate_BorrowingWithEditionHavingNullBook_ShouldFailValidation()
        {
            // Arrange
            var borrowing = _fixture.Build<Borrowing>()
                .With(b => b.BorrowDate, DateTime.Now)
                .With(b => b.DueDate, DateTime.Now.AddDays(30))
                .Without(b => b.ReturnDate)
                .Create();

            var validBookCopy = _bookCopyFactory("Valid Book", "Test Domain");
            var bookCopyWithNullBook = new BookCopy(isLectureRoomOnly: false)
            {
                Id = Guid.NewGuid(),
                Edition = new Edition
                {
                    Id = Guid.NewGuid(),
                    Book = null,
                    BookType = new BookType { Id = Guid.NewGuid(), Name = "Hardcover" }
                }
            };

            borrowing.BookCopies.Add(validBookCopy);
            borrowing.BookCopies.Add(bookCopyWithNullBook);

            // Act
            var result = Validate(borrowing);

            // Assert
            result.IsValid.Should().BeFalse();
        }
    }
}
