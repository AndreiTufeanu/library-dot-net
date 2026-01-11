using AutoFixture;
using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
    public class DomainHelperServiceTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly DomainHelperService _service;

        private readonly Mock<IDomainRepository> _domainRepositoryMock;

        public DomainHelperServiceTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _domainRepositoryMock = new Mock<IDomainRepository>();

            _unitOfWorkMock.Setup(u => u.Domains).Returns(_domainRepositoryMock.Object);

            _service = new DomainHelperService(_unitOfWorkMock.Object);
        }

        private Domain CreateDomain(Guid? id = null, Domain parent = null)
        {
            var domain = _fixture.Build<Domain>()
                .With(d => d.Id, id ?? Guid.NewGuid())
                .Without(d => d.ParentDomain)
                .Create();

            if (parent != null)
            {
                domain.ParentDomain = parent;
            }

            return domain;
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WhenUnitOfWorkIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            IUnitOfWork unitOfWork = null;

            // Act
            Action act = () => new DomainHelperService(unitOfWork);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("*unitOfWork*");
        }

        #endregion

        #region PopulateAncestorDomainIdsAsync Tests

        [TestMethod]
        public async Task PopulateAncestorDomainIdsAsync_WhenDomainHasMultipleAncestors_ShouldAddAllAncestorIds()
        {
            // Arrange
            var grandparentId = Guid.NewGuid();
            var parentId = Guid.NewGuid();
            var domainId = Guid.NewGuid();

            var grandparentDomain = CreateDomain(grandparentId);
            var parentDomain = CreateDomain(parentId, parent: grandparentDomain);
            var domain = CreateDomain(domainId, parent: parentDomain);

            var domainIds = new HashSet<Guid>();

            _domainRepositoryMock.SetupSequence(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(domain)
                .ReturnsAsync(parentDomain)
                .ReturnsAsync(grandparentDomain)
                .ReturnsAsync((Domain)null);

            // Act
            await _service.PopulateAncestorDomainIdsAsync(domainId, domainIds);

            // Assert
            domainIds.Should().HaveCount(2);
            domainIds.Should().Contain(parentId);
            domainIds.Should().Contain(grandparentId);
            domainIds.Should().NotContain(domainId);
        }

        [TestMethod]
        public async Task PopulateAncestorDomainIdsAsync_WhenDomainNotFound_ShouldNotThrowAndNotAddIds()
        {
            // Arrange
            var domainId = Guid.NewGuid();
            var domainIds = new HashSet<Guid>();

            _domainRepositoryMock.Setup(r => r.GetByIdAsync(domainId))
                .ReturnsAsync((Domain)null);

            // Act
            await _service.PopulateAncestorDomainIdsAsync(domainId, domainIds);

            // Assert
            domainIds.Should().BeEmpty();
        }

        #endregion

        #region PopulateDescendantDomainIdsAsync Tests

        [TestMethod]
        public async Task PopulateDescendantDomainIdsAsync_WhenDomainHasDirectSubdomains_ShouldAddAllSubdomainIds()
        {
            // Arrange
            var domainId = Guid.NewGuid();
            var subdomain1 = CreateDomain(Guid.NewGuid());
            var subdomain2 = CreateDomain(Guid.NewGuid());
            var subdomains = new List<Domain> { subdomain1, subdomain2 };

            var domainIds = new HashSet<Guid>();

            _domainRepositoryMock.Setup(r => r.GetSubdomainsAsync(domainId))
                .ReturnsAsync(subdomains);
            _domainRepositoryMock.Setup(r => r.GetSubdomainsAsync(subdomain1.Id))
                .ReturnsAsync(new List<Domain>());
            _domainRepositoryMock.Setup(r => r.GetSubdomainsAsync(subdomain2.Id))
                .ReturnsAsync(new List<Domain>());

            // Act
            await _service.PopulateDescendantDomainIdsAsync(domainId, domainIds);

            // Assert
            domainIds.Should().HaveCount(2);
            domainIds.Should().Contain(subdomain1.Id);
            domainIds.Should().Contain(subdomain2.Id);
            domainIds.Should().NotContain(domainId);
        }

        [TestMethod]
        public async Task PopulateDescendantDomainIdsAsync_WhenCollectionAlreadyContainsSomeIds_ShouldNotAddDuplicates()
        {
            // Arrange
            var domainId = Guid.NewGuid();
            var existingId = Guid.NewGuid();
            var newId = Guid.NewGuid();

            var existingDomain = CreateDomain(existingId);
            var newDomain = CreateDomain(newId);
            var subdomains = new List<Domain> { existingDomain, newDomain };

            var domainIds = new HashSet<Guid> { existingId };

            _domainRepositoryMock.Setup(r => r.GetSubdomainsAsync(domainId))
                .ReturnsAsync(subdomains);
            _domainRepositoryMock.Setup(r => r.GetSubdomainsAsync(existingId))
                .ReturnsAsync(new List<Domain>());
            _domainRepositoryMock.Setup(r => r.GetSubdomainsAsync(newId))
                .ReturnsAsync(new List<Domain>());

            // Act
            await _service.PopulateDescendantDomainIdsAsync(domainId, domainIds);

            // Assert
            domainIds.Should().HaveCount(2);
            domainIds.Should().Contain(existingId);
            domainIds.Should().Contain(newId);
        }

        #endregion
    }
}
