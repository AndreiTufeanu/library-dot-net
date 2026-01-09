using AutoFixture;
using DomainModel.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLayer.Exceptions;
using ServiceLayer.ServiceContracts;
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
        private readonly BookHelperService _service;

        public BookHelperServiceTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _configServiceMock = new Mock<IConfigurationSettingService>();
            _service = new BookHelperService(_configServiceMock.Object);
        }

        [TestMethod]
        public void Constructor_NullConfigurationService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() =>
                new BookHelperService(null));
        }

        [TestMethod]
        public async Task ValidateMaxDomainsPerBookAsync_WithinLimit_ShouldNotThrowException()
        {
            // Arrange
            const int maxDomains = 5;
            _configServiceMock.Setup(x => x.GetMaxDomainsPerBookAsync())
                .ReturnsAsync(maxDomains);

            var domains = new List<Domain>
            {
                new Domain { Id = Guid.NewGuid(), Name = "Domain 1" },
                new Domain { Id = Guid.NewGuid(), Name = "Domain 2" },
                new Domain { Id = Guid.NewGuid(), Name = "Domain 3" }
            };

            // Act & Assert
            await _service.Invoking(s => s.ValidateMaxDomainsPerBookAsync(domains))
                .Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task ValidateMaxDomainsPerBookAsync_AtLimit_ShouldNotThrowException()
        {
            // Arrange
            const int maxDomains = 5;
            _configServiceMock.Setup(x => x.GetMaxDomainsPerBookAsync())
                .ReturnsAsync(maxDomains);

            var domains = new List<Domain>();
            for (int i = 0; i < maxDomains; i++)
            {
                domains.Add(new Domain { Id = Guid.NewGuid(), Name = $"Domain {i}" });
            }

            // Act & Assert
            await _service.Invoking(s => s.ValidateMaxDomainsPerBookAsync(domains))
                .Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task ValidateMaxDomainsPerBookAsync_ExceedsLimit_ShouldThrowValidationException()
        {
            // Arrange
            const int maxDomains = 5;
            _configServiceMock.Setup(x => x.GetMaxDomainsPerBookAsync())
                .ReturnsAsync(maxDomains);

            var domains = new List<Domain>();
            for (int i = 0; i < maxDomains + 1; i++)
            {
                domains.Add(new Domain { Id = Guid.NewGuid(), Name = $"Domain {i}" });
            }

            // Act & Assert
            await _service.Invoking(s => s.ValidateMaxDomainsPerBookAsync(domains))
                .Should().ThrowAsync<AggregateValidationException>()
                .WithMessage($"*A book cannot belong to more than {maxDomains} domains. Current count: {maxDomains + 1}");
        }

        [TestMethod]
        public async Task ValidateMaxDomainsPerBookAsync_ExceedsLimitByMany_ShouldThrowValidationExceptionWithCorrectCount()
        {
            // Arrange
            const int maxDomains = 5;
            const int actualCount = 10;
            _configServiceMock.Setup(x => x.GetMaxDomainsPerBookAsync())
                .ReturnsAsync(maxDomains);

            var domains = new List<Domain>();
            for (int i = 0; i < actualCount; i++)
            {
                domains.Add(new Domain { Id = Guid.NewGuid(), Name = $"Domain {i}" });
            }

            // Act & Assert
            await _service.Invoking(s => s.ValidateMaxDomainsPerBookAsync(domains))
                .Should().ThrowAsync<AggregateValidationException>()
                .WithMessage($"*A book cannot belong to more than {maxDomains} domains. Current count: {actualCount}");
        }

        [TestMethod]
        public async Task ValidateMaxDomainsPerBookAsync_NullDomains_ShouldThrowArgumentNullException()
        {
            // Arrange
            ICollection<Domain> domains = null;

            // Act & Assert
            await _service.Invoking(s => s.ValidateMaxDomainsPerBookAsync(domains))
                .Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public async Task ValidateMaxDomainsPerBookAsync_ZeroMaxDomains_ShouldThrowValidationExceptionForAnyDomains()
        {
            // Arrange
            const int maxDomains = 0;
            _configServiceMock.Setup(x => x.GetMaxDomainsPerBookAsync())
                .ReturnsAsync(maxDomains);

            var domains = new List<Domain>
            {
                new Domain { Id = Guid.NewGuid(), Name = "Domain 1" }
            };

            // Act & Assert
            await _service.Invoking(s => s.ValidateMaxDomainsPerBookAsync(domains))
                .Should().ThrowAsync<AggregateValidationException>()
                .WithMessage($"*A book cannot belong to more than {maxDomains} domains. Current count: 1");
        }

        [TestMethod]
        public async Task ValidateMaxDomainsPerBookAsync_EmptyDomainCollection_ShouldNotThrowException()
        {
            // Arrange
            const int maxDomains = 5;
            _configServiceMock.Setup(x => x.GetMaxDomainsPerBookAsync())
                .ReturnsAsync(maxDomains);

            var domains = new List<Domain>();

            // Act & Assert
            await _service.Invoking(s => s.ValidateMaxDomainsPerBookAsync(domains))
                .Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task ValidateMaxDomainsPerBookAsync_ConfigurationServiceThrows_ShouldPropagateException()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Database connection failed");
            _configServiceMock.Setup(x => x.GetMaxDomainsPerBookAsync())
                .ThrowsAsync(expectedException);

            var domains = new List<Domain> { new Domain() };

            // Act & Assert
            await _service.Invoking(s => s.ValidateMaxDomainsPerBookAsync(domains))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage(expectedException.Message);
        }
    }
}
