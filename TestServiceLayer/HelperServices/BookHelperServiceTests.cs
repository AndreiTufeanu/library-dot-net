using AutoFixture;
using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
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
    public class BookHelperServiceTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IConfigurationSettingService> _configServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IDomainHelperService> _domainHelperServiceMock;
        private readonly BookHelperService _service;
        private readonly Mock<IBookRepository> _bookRepositoryMock;

        public BookHelperServiceTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _configServiceMock = new Mock<IConfigurationSettingService>();
            _domainHelperServiceMock = new Mock<IDomainHelperService>();
            _bookRepositoryMock = new Mock<IBookRepository>();

            _unitOfWorkMock.Setup(u => u.Books).Returns(_bookRepositoryMock.Object);
            _service = new BookHelperService(_configServiceMock.Object, _unitOfWorkMock.Object, _domainHelperServiceMock.Object);
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithNullConfigurationService_ShouldThrow()
        {
            // Act & Assert
            Action act = () => new BookHelperService(null, Mock.Of<IUnitOfWork>(), Mock.Of<IDomainHelperService>());
            act.Should().Throw<ArgumentNullException>().WithMessage("*configurationService*");
        }

        [TestMethod]
        public void Constructor_WithNullUnitOfWork_ShouldThrow()
        {
            // Act & Assert
            Action act = () => new BookHelperService(Mock.Of<IConfigurationSettingService>(), null, Mock.Of<IDomainHelperService>());
            act.Should().Throw<ArgumentNullException>().WithMessage("*unitOfWork*");
        }

        [TestMethod]
        public void Constructor_WithNullDomainHelperService_ShouldThrow()
        {
            // Act & Assert
            Action act = () => new BookHelperService(Mock.Of<IConfigurationSettingService>(), Mock.Of<IUnitOfWork>(), null);
            act.Should().Throw<ArgumentNullException>().WithMessage("*domainHelperService*");
        }

        #endregion

        #region ValidateMaxDomainsPerBookAsync Tests

        [TestMethod]
        public async Task ValidateMaxDomainsPerBookAsync_WithValidDomainCount_ShouldNotThrow()
        {
            // Arrange
            var domains = _fixture.CreateMany<Domain>(3).ToList();
            _configServiceMock.Setup(c => c.GetMaxDomainsPerBookAsync()).ReturnsAsync(5);

            // Act
            Func<Task> act = async () => await _service.ValidateMaxDomainsPerBookAsync(domains);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task ValidateMaxDomainsPerBookAsync_WithDomainCountAtLimit_ShouldNotThrow()
        {
            // Arrange
            var domains = _fixture.CreateMany<Domain>(5).ToList();
            _configServiceMock.Setup(c => c.GetMaxDomainsPerBookAsync()).ReturnsAsync(5);

            // Act
            Func<Task> act = async () => await _service.ValidateMaxDomainsPerBookAsync(domains);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task ValidateMaxDomainsPerBookAsync_WithDomainCountExceedingLimit_ShouldThrowValidationException()
        {
            // Arrange
            var domains = _fixture.CreateMany<Domain>(6).ToList();
            _configServiceMock.Setup(c => c.GetMaxDomainsPerBookAsync()).ReturnsAsync(5);

            // Act
            Func<Task> act = async () => await _service.ValidateMaxDomainsPerBookAsync(domains);

            // Assert
            await act.Should().ThrowAsync<AggregateValidationException>();
        }

        [TestMethod]
        public async Task ValidateMaxDomainsPerBookAsync_WithNullDomains_ShouldThrow()
        {
            // Act & Assert
            Func<Task> act = async () => await _service.ValidateMaxDomainsPerBookAsync(null);
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public async Task ValidateMaxDomainsPerBookAsync_WithEmptyDomains_ShouldNotThrow()
        {
            // Arrange
            _configServiceMock.Setup(c => c.GetMaxDomainsPerBookAsync()).ReturnsAsync(5);

            // Act
            Func<Task> act = async () => await _service.ValidateMaxDomainsPerBookAsync(new List<Domain>());

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task ValidateMaxDomainsPerBookAsync_WithZeroMaxDomains_ShouldThrowForAnyDomain()
        {
            // Arrange
            var domains = _fixture.CreateMany<Domain>(1).ToList();
            _configServiceMock.Setup(c => c.GetMaxDomainsPerBookAsync()).ReturnsAsync(0);

            // Act
            Func<Task> act = async () => await _service.ValidateMaxDomainsPerBookAsync(domains);

            // Assert
            await act.Should().ThrowAsync<AggregateValidationException>();
        }

        #endregion

        #region GetCompleteDomainHierarchyForBookAsync Tests

        [TestMethod]
        public async Task GetCompleteDomainHierarchyForBookAsync_WhenBookNotFound_ShouldReturnEmptySet()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync((Book)null);

            // Act
            var result = await _service.GetCompleteDomainHierarchyForBookAsync(bookId);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetCompleteDomainHierarchyForBookAsync_WhenBookHasNoDomains_ShouldReturnEmptySet()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var book = _fixture.Build<Book>()
                .FromFactory(() => new Book(initialCopies: 10))
                .Create();

            _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(book);

            // Act
            var result = await _service.GetCompleteDomainHierarchyForBookAsync(bookId);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetCompleteDomainHierarchyForBookAsync_WhenBookHasSingleDomain_ShouldReturnCompleteHierarchy()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var domainId = Guid.NewGuid();

            var domain = _fixture.Build<Domain>()
                .With(d => d.Id, domainId)
                .Create();
            var book = _fixture.Build<Book>()
                .FromFactory(() => new Book(initialCopies: 10))
                .Create();

            book.Domains.Add(domain);

            var ancestorId = Guid.NewGuid();
            var descendantId = Guid.NewGuid();

            _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(book);

            _domainHelperServiceMock.Setup(s => s.PopulateAncestorDomainIdsAsync(domainId, It.IsAny<HashSet<Guid>>()))
                .Callback<Guid, HashSet<Guid>>((_, set) => set.Add(ancestorId));

            _domainHelperServiceMock.Setup(s => s.PopulateDescendantDomainIdsAsync(domainId, It.IsAny<HashSet<Guid>>()))
                .Callback<Guid, HashSet<Guid>>((_, set) => set.Add(descendantId));

            // Act
            var result = await _service.GetCompleteDomainHierarchyForBookAsync(bookId);

            // Assert
            result.Should().Contain(new[] { domainId, ancestorId, descendantId });
        }

        [TestMethod]
        public async Task GetCompleteDomainHierarchyForBookAsync_WhenBookHasMultipleDomains_ShouldCombineHierarchies()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var domain1Id = Guid.NewGuid();
            var domain2Id = Guid.NewGuid();

            var domain1 = _fixture.Build<Domain>()
                .With(d => d.Id, domain1Id)
                .Create();
            var domain2 = _fixture.Build<Domain>()
                .With(d => d.Id, domain2Id)
                .Create();

            var book = _fixture.Build<Book>()
                .FromFactory(() => new Book(initialCopies: 10))
                .Create();

            book.Domains.Add(domain1);
            book.Domains.Add(domain2);

            var sharedAncestorId = Guid.NewGuid();

            _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(book);

            _domainHelperServiceMock.Setup(s => s.PopulateAncestorDomainIdsAsync(It.IsAny<Guid>(), It.IsAny<HashSet<Guid>>()))
                .Callback<Guid, HashSet<Guid>>((_, set) => set.Add(sharedAncestorId));

            // Act
            var result = await _service.GetCompleteDomainHierarchyForBookAsync(bookId);

            // Assert
            result.Should().Contain(domain1Id);
            result.Should().Contain(domain2Id);
            result.Should().Contain(sharedAncestorId);
            result.Should().HaveCount(3);
        }

        [TestMethod]
        public async Task GetCompleteDomainHierarchyForBookAsync_WhenDomainHelperThrowsException_ShouldPropagate()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var domain = _fixture.Create<Domain>();
            var book = _fixture.Build<Book>()
                .FromFactory(() => new Book(initialCopies: 10))
                .Create();
            book.Domains.Add(domain);

            _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(book);
            _domainHelperServiceMock.Setup(s => s.PopulateAncestorDomainIdsAsync(It.IsAny<Guid>(), It.IsAny<HashSet<Guid>>()))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            // Act
            Func<Task> act = async () => await _service.GetCompleteDomainHierarchyForBookAsync(bookId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Test exception");
        }

        #endregion
    }
}
