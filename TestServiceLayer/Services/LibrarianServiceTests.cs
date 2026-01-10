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
    public class LibrarianServiceTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<LibrarianService>> _loggerMock;
        private readonly LibrarianService _service;

        private readonly Mock<ILibrarianRepository> _librarianRepositoryMock;
        private readonly Mock<IReaderRepository> _readerRepositoryMock;
        private readonly Mock<IBorrowingRepository> _borrowingRepositoryMock;

        public LibrarianServiceTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<LibrarianService>>();

            // Setup repository mocks
            _librarianRepositoryMock = new Mock<ILibrarianRepository>();
            _readerRepositoryMock = new Mock<IReaderRepository>();
            _borrowingRepositoryMock = new Mock<IBorrowingRepository>();

            // Setup unit of work to return repository mocks
            _unitOfWorkMock.Setup(u => u.Librarians).Returns(_librarianRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.Readers).Returns(_readerRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.Borrowings).Returns(_borrowingRepositoryMock.Object);

            _service = new LibrarianService(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        private Librarian CreateValidLibrarian()
        {
            var reader = _fixture.Build<Reader>()
                .With(r => r.FirstName, "John")
                .With(r => r.LastName, "Doe")
                .With(r => r.Email, "john.doe@library.com")
                .Create();

            return _fixture.Build<Librarian>()
                .With(l => l.ReaderDetails, reader)
                .Create();
        }

        private Reader CreateValidReader()
        {
            return _fixture.Build<Reader>()
                .With(r => r.FirstName, "Jane")
                .With(r => r.LastName, "Smith")
                .With(r => r.Email, "jane.smith@library.com")
                .Create();
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WhenUnitOfWorkIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            IUnitOfWork unitOfWork = null;
            ILogger<LibrarianService> logger = Mock.Of<ILogger<LibrarianService>>();

            // Act
            Action act = () => new LibrarianService(unitOfWork, logger);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*unitOfWork*");
        }

        [TestMethod]
        public void Constructor_WhenLoggerIsNull_ShouldNotThrow()
        {
            // Arrange
            var unitOfWork = Mock.Of<IUnitOfWork>();
            ILogger<LibrarianService> logger = null;

            // Act
            Action act = () => new LibrarianService(unitOfWork, logger);

            // Assert
            act.Should().NotThrow();
        }

        #endregion

        #region CreateAsync Tests

        [TestMethod]
        public async Task CreateAsync_ValidLibrarian_ShouldReturnSuccess()
        {
            // Arrange
            var librarian = CreateValidLibrarian();
            var addedLibrarian = _fixture.Build<Librarian>()
                .With(l => l.Id, librarian.Id)
                .Create();

            _readerRepositoryMock.Setup(r => r.GetByIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync(librarian.ReaderDetails);
            _librarianRepositoryMock.Setup(r => r.GetByReaderIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync((Librarian)null);
            _librarianRepositoryMock.Setup(r => r.AddAsync(librarian))
                .ReturnsAsync(addedLibrarian);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(librarian);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be(addedLibrarian);
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.ValidationErrors.Should().BeEmpty();
        }

        [TestMethod]
        public async Task CreateAsync_ValidLibrarian_ShouldCallRepositoryMethods()
        {
            // Arrange
            var librarian = CreateValidLibrarian();

            _readerRepositoryMock.Setup(r => r.GetByIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync(librarian.ReaderDetails);
            _librarianRepositoryMock.Setup(r => r.GetByReaderIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync((Librarian)null);
            _librarianRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Librarian>()))
                .ReturnsAsync(librarian);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _service.CreateAsync(librarian);

            // Assert
            _readerRepositoryMock.Verify(r => r.GetByIdAsync(librarian.ReaderDetails.Id), Times.Once);
            _librarianRepositoryMock.Verify(r => r.GetByReaderIdAsync(librarian.ReaderDetails.Id), Times.Once);
            _librarianRepositoryMock.Verify(r => r.AddAsync(librarian), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task CreateAsync_ReaderNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var librarian = CreateValidLibrarian();

            _readerRepositoryMock.Setup(r => r.GetByIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync((Reader)null);

            // Act
            var result = await _service.CreateAsync(librarian);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Reader with ID '{librarian.ReaderDetails.Id}' not found");
        }

        [TestMethod]
        public async Task CreateAsync_ReaderAlreadyLibrarian_ShouldReturnValidationError()
        {
            // Arrange
            var librarian = CreateValidLibrarian();
            var existingLibrarian = CreateValidLibrarian();

            _readerRepositoryMock.Setup(r => r.GetByIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync(librarian.ReaderDetails);
            _librarianRepositoryMock.Setup(r => r.GetByReaderIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync(existingLibrarian);

            // Act
            var result = await _service.CreateAsync(librarian);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain($"Reader '{librarian.ReaderDetails.FirstName} {librarian.ReaderDetails.LastName}' is already a librarian");
        }

        #endregion

        #region GetByIdAsync Tests

        [TestMethod]
        public async Task GetByIdAsync_LibrarianExists_ShouldReturnLibrarian()
        {
            // Arrange
            var librarianId = Guid.NewGuid();
            var librarian = _fixture.Build<Librarian>()
                .With(l => l.Id, librarianId)
                .Create();

            _librarianRepositoryMock.Setup(r => r.GetByIdAsync(librarianId))
                .ReturnsAsync(librarian);

            // Act
            var result = await _service.GetByIdAsync(librarianId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be(librarian);
        }

        [TestMethod]
        public async Task GetByIdAsync_LibrarianNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var librarianId = Guid.NewGuid();
            _librarianRepositoryMock.Setup(r => r.GetByIdAsync(librarianId))
                .ReturnsAsync((Librarian)null);

            // Act
            var result = await _service.GetByIdAsync(librarianId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Librarian with ID '{librarianId}' not found");
        }

        #endregion

        #region GetAllAsync Tests

        [TestMethod]
        public async Task GetAllAsync_LibrariansExist_ShouldReturnAllLibrarians()
        {
            // Arrange
            var librarians = _fixture.CreateMany<Librarian>(3).ToList();
            _librarianRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(librarians);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(librarians);
        }

        [TestMethod]
        public async Task GetAllAsync_NoLibrarians_ShouldReturnEmptyList()
        {
            // Arrange
            _librarianRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Librarian>());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region UpdateAsync Tests

        [TestMethod]
        public async Task UpdateAsync_ValidLibrarian_ShouldReturnSuccess()
        {
            // Arrange
            var librarian = CreateValidLibrarian();

            _librarianRepositoryMock.Setup(r => r.ExistsAsync(librarian.Id))
                .ReturnsAsync(true);
            _readerRepositoryMock.Setup(r => r.GetByIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync(librarian.ReaderDetails);
            _librarianRepositoryMock.Setup(r => r.GetByReaderIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync(librarian);
            _librarianRepositoryMock.Setup(r => r.UpdateAsync(librarian))
                .ReturnsAsync(librarian);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.UpdateAsync(librarian);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [TestMethod]
        public async Task UpdateAsync_ChangingToDifferentReader_ShouldCallRepositoryMethods()
        {
            // Arrange
            var librarian = CreateValidLibrarian();
            var newReader = CreateValidReader();

            librarian.ReaderDetails = newReader;

            _librarianRepositoryMock.Setup(r => r.ExistsAsync(librarian.Id))
                .ReturnsAsync(true);
            _readerRepositoryMock.Setup(r => r.GetByIdAsync(newReader.Id))
                .ReturnsAsync(newReader);
            _librarianRepositoryMock.Setup(r => r.GetByReaderIdAsync(newReader.Id))
                .ReturnsAsync((Librarian)null);
            _librarianRepositoryMock.Setup(r => r.UpdateAsync(librarian))
                .ReturnsAsync(librarian);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.UpdateAsync(librarian);

            // Assert
            result.Success.Should().BeTrue();
            _readerRepositoryMock.Verify(r => r.GetByIdAsync(newReader.Id), Times.Once);
            _librarianRepositoryMock.Verify(r => r.GetByReaderIdAsync(newReader.Id), Times.Once);
        }

        [TestMethod]
        public async Task UpdateAsync_LibrarianNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var librarian = CreateValidLibrarian();
            _librarianRepositoryMock.Setup(r => r.ExistsAsync(librarian.Id))
                .ReturnsAsync(false);

            // Act
            var result = await _service.UpdateAsync(librarian);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Librarian with ID '{librarian.Id}' not found");
        }

        [TestMethod]
        public async Task UpdateAsync_ReaderNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var librarian = CreateValidLibrarian();

            _librarianRepositoryMock.Setup(r => r.ExistsAsync(librarian.Id))
                .ReturnsAsync(true);
            _readerRepositoryMock.Setup(r => r.GetByIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync((Reader)null);

            // Act
            var result = await _service.UpdateAsync(librarian);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Reader with ID '{librarian.ReaderDetails.Id}' not found");
        }

        [TestMethod]
        public async Task UpdateAsync_ReaderAlreadyAssignedToAnotherLibrarian_ShouldReturnValidationError()
        {
            // Arrange
            var librarian = CreateValidLibrarian();
            var otherLibrarian = CreateValidLibrarian();
            otherLibrarian.Id = Guid.NewGuid();

            _librarianRepositoryMock.Setup(r => r.ExistsAsync(librarian.Id))
                .ReturnsAsync(true);
            _readerRepositoryMock.Setup(r => r.GetByIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync(librarian.ReaderDetails);
            _librarianRepositoryMock.Setup(r => r.GetByReaderIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync(otherLibrarian);

            // Act
            var result = await _service.UpdateAsync(librarian);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().ContainSingle()
                .Which.Should().Contain($"Reader '{librarian.ReaderDetails.FirstName} {librarian.ReaderDetails.LastName}' is already assigned to another librarian");
        }

        [TestMethod]
        public async Task UpdateAsync_KeepSameReader_ShouldSucceed()
        {
            // Arrange
            var librarian = CreateValidLibrarian();

            _librarianRepositoryMock.Setup(r => r.ExistsAsync(librarian.Id))
                .ReturnsAsync(true);
            _readerRepositoryMock.Setup(r => r.GetByIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync(librarian.ReaderDetails);
            _librarianRepositoryMock.Setup(r => r.GetByReaderIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync(librarian);
            _librarianRepositoryMock.Setup(r => r.UpdateAsync(librarian))
                .ReturnsAsync(librarian);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.UpdateAsync(librarian);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        #endregion

        #region DeleteAsync Tests

        [TestMethod]
        public async Task DeleteAsync_LibrarianWithoutProcessedLoans_ShouldReturnSuccess()
        {
            // Arrange
            var librarianId = Guid.NewGuid();
            var librarian = CreateValidLibrarian();

            _librarianRepositoryMock.Setup(r => r.GetByIdAsync(librarianId))
                .ReturnsAsync(librarian);
            _librarianRepositoryMock.Setup(r => r.DeleteAsync(librarianId))
                .ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.DeleteAsync(librarianId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
            _librarianRepositoryMock.Verify(r => r.DeleteAsync(librarianId), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task DeleteAsync_LibrarianNotFound_ShouldReturnNotFoundError()
        {
            // Arrange
            var librarianId = Guid.NewGuid();
            _librarianRepositoryMock.Setup(r => r.GetByIdAsync(librarianId))
                .ReturnsAsync((Librarian)null);

            // Act
            var result = await _service.DeleteAsync(librarianId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Librarian with ID '{librarianId}' not found");
            _librarianRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task DeleteAsync_LibrarianWithProcessedLoans_ShouldReturnBusinessRuleError()
        {
            // Arrange
            var librarianId = Guid.NewGuid();
            var librarian = CreateValidLibrarian();

            var borrowings = _fixture.CreateMany<Borrowing>(2).ToList();
            foreach (var borrowing in borrowings)
            {
                librarian.ProcessedLoans.Add(borrowing);
            }

            _librarianRepositoryMock.Setup(r => r.GetByIdAsync(librarianId))
                .ReturnsAsync(librarian);

            // Act
            var result = await _service.DeleteAsync(librarianId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Cannot delete a librarian that has processed loans. Reassign loans first");
            _librarianRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [TestMethod]
        public async Task DeleteAsync_LibrarianWithEmptyProcessedLoansCollection_ShouldSucceed()
        {
            // Arrange
            var librarianId = Guid.NewGuid();
            var librarian = CreateValidLibrarian();

            _librarianRepositoryMock.Setup(r => r.GetByIdAsync(librarianId))
                .ReturnsAsync(librarian);
            _librarianRepositoryMock.Setup(r => r.DeleteAsync(librarianId))
                .ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.DeleteAsync(librarianId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [TestMethod]
        public async Task DeleteAsync_DeleteFails_ShouldReturnFailure()
        {
            // Arrange
            var librarianId = Guid.NewGuid();
            var librarian = CreateValidLibrarian();

            _librarianRepositoryMock.Setup(r => r.GetByIdAsync(librarianId))
                .ReturnsAsync(librarian);
            _librarianRepositoryMock.Setup(r => r.DeleteAsync(librarianId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteAsync(librarianId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Failed to delete librarian");
            _librarianRepositoryMock.Verify(r => r.DeleteAsync(librarianId), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        #endregion

        #region ExistsAsync Tests

        [TestMethod]
        public async Task ExistsAsync_LibrarianExists_ShouldReturnTrue()
        {
            // Arrange
            var librarianId = Guid.NewGuid();
            _librarianRepositoryMock.Setup(r => r.ExistsAsync(librarianId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ExistsAsync(librarianId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [TestMethod]
        public async Task ExistsAsync_LibrarianDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var librarianId = Guid.NewGuid();
            _librarianRepositoryMock.Setup(r => r.ExistsAsync(librarianId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.ExistsAsync(librarianId);

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
            var librarian = CreateValidLibrarian();

            _readerRepositoryMock.Setup(r => r.GetByIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync(librarian.ReaderDetails);
            _librarianRepositoryMock.Setup(r => r.GetByReaderIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync((Librarian)null);
            _librarianRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Librarian>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act
            var result = await _service.CreateAsync(librarian);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred");
        }

        [TestMethod]
        public async Task UpdateAsync_RepositoryThrowsException_ShouldReturnFailureWithErrorMessage()
        {
            // Arrange
            var librarian = CreateValidLibrarian();

            _librarianRepositoryMock.Setup(r => r.ExistsAsync(librarian.Id))
                .ReturnsAsync(true);
            _readerRepositoryMock.Setup(r => r.GetByIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync(librarian.ReaderDetails);
            _librarianRepositoryMock.Setup(r => r.GetByReaderIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync(librarian);
            _librarianRepositoryMock.Setup(r => r.UpdateAsync(librarian))
                .ThrowsAsync(new InvalidOperationException("Database constraint violation"));

            // Act
            var result = await _service.UpdateAsync(librarian);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred");
        }

        [TestMethod]
        public async Task DeleteAsync_RepositoryThrowsException_ShouldReturnFailureWithErrorMessage()
        {
            // Arrange
            var librarianId = Guid.NewGuid();
            var librarian = CreateValidLibrarian();

            _librarianRepositoryMock.Setup(r => r.GetByIdAsync(librarianId))
                .ReturnsAsync(librarian);
            _librarianRepositoryMock.Setup(r => r.DeleteAsync(librarianId))
                .ThrowsAsync(new InvalidOperationException("Database constraint violation"));

            // Act
            var result = await _service.DeleteAsync(librarianId);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred");
        }

        #endregion

        #region Edge Cases and Boundary Tests

        [TestMethod]
        public async Task CreateAsync_WithLibrarianHavingNullReaderDetails_ShouldPassValidation()
        {
            // Arrange
            var librarian = CreateValidLibrarian();
            librarian.ReaderDetails = null;

            // Act
            var result = await _service.CreateAsync(librarian);

            // Assert
            result.Success.Should().BeTrue();
        }

        [TestMethod]
        public async Task CreateAsync_WithEmptyGuidReaderId_ShouldReturnNotFoundError()
        {
            // Arrange
            var librarian = CreateValidLibrarian();
            librarian.ReaderDetails.Id = Guid.Empty;

            _readerRepositoryMock.Setup(r => r.GetByIdAsync(Guid.Empty))
                .ReturnsAsync((Reader)null);

            // Act
            var result = await _service.CreateAsync(librarian);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Reader with ID '{Guid.Empty}' not found");
        }

        [TestMethod]
        public async Task UpdateAsync_WithNullReaderDetails_ShouldPassValidation()
        {
            // Arrange
            var librarian = CreateValidLibrarian();
            librarian.ReaderDetails = null;

            _librarianRepositoryMock.Setup(r => r.ExistsAsync(librarian.Id))
                .ReturnsAsync(true);

            // Act
            var result = await _service.UpdateAsync(librarian);

            // Assert
            result.Success.Should().BeTrue();
        }

        [TestMethod]
        public async Task UpdateAsync_KeepSameLibrarianIdButChangeReaderDetails_ShouldValidateReaderUniqueness()
        {
            // Arrange
            var librarian = CreateValidLibrarian();
            var newReader = CreateValidReader();
            librarian.ReaderDetails = newReader;

            _librarianRepositoryMock.Setup(r => r.ExistsAsync(librarian.Id))
                .ReturnsAsync(true);
            _readerRepositoryMock.Setup(r => r.GetByIdAsync(newReader.Id))
                .ReturnsAsync(newReader);
            _librarianRepositoryMock.Setup(r => r.GetByReaderIdAsync(newReader.Id))
                .ReturnsAsync((Librarian)null);
            _librarianRepositoryMock.Setup(r => r.UpdateAsync(librarian))
                .ReturnsAsync(librarian);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.UpdateAsync(librarian);

            // Assert
            result.Success.Should().BeTrue();
            _librarianRepositoryMock.Verify(r => r.GetByReaderIdAsync(newReader.Id), Times.Once);
        }

        [TestMethod]
        public async Task GetAllAsync_WithLibrariansHavingReaderDetails_ShouldIncludeReaderDetails()
        {
            // Arrange
            var librarians = _fixture.Build<Librarian>()
                .With(l => l.ReaderDetails, CreateValidReader())
                .CreateMany(2)
                .ToList();

            _librarianRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(librarians);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(librarians);
            result.Data.All(l => l.ReaderDetails != null).Should().BeTrue();
        }

        #endregion

        #region Business Logic Specific Tests

        [TestMethod]
        public async Task CreateAsync_WithLibrarianHavingProcessedLoans_ShouldSucceed()
        {
            // Arrange
            var librarian = CreateValidLibrarian();
            var borrowings = _fixture.CreateMany<Borrowing>(3).ToList();
            foreach (var borrowing in borrowings)
            {
                librarian.ProcessedLoans.Add(borrowing);
            }

            var addedLibrarian = _fixture.Build<Librarian>()
                .With(l => l.Id, librarian.Id)
                .Create();

            _readerRepositoryMock.Setup(r => r.GetByIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync(librarian.ReaderDetails);
            _librarianRepositoryMock.Setup(r => r.GetByReaderIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync((Librarian)null);
            _librarianRepositoryMock.Setup(r => r.AddAsync(librarian))
                .ReturnsAsync(addedLibrarian);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(librarian);

            // Assert
            result.Success.Should().BeTrue();
        }

        [TestMethod]
        public async Task UpdateAsync_WithLibrarianHavingNullProcessedLoans_ShouldSucceed()
        {
            // Arrange
            var librarian = CreateValidLibrarian();

            _librarianRepositoryMock.Setup(r => r.ExistsAsync(librarian.Id))
                .ReturnsAsync(true);
            _readerRepositoryMock.Setup(r => r.GetByIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync(librarian.ReaderDetails);
            _librarianRepositoryMock.Setup(r => r.GetByReaderIdAsync(librarian.ReaderDetails.Id))
                .ReturnsAsync(librarian);
            _librarianRepositoryMock.Setup(r => r.UpdateAsync(librarian))
                .ReturnsAsync(librarian);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.UpdateAsync(librarian);

            // Assert
            result.Success.Should().BeTrue();
        }

        #endregion
    }
}
