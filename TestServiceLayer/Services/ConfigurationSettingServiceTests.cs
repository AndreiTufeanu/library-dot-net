using AutoFixture;
using DomainModel.Entities;
using DomainModel.RepositoryContracts;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLayer.Helpers;
using ServiceLayer.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServiceLayer.Services
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ConfigurationSettingServiceTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IConfigurationSettingRepository> _repositoryMock;
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly ConfigurationSettingService _service;

        public ConfigurationSettingServiceTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _repositoryMock = new Mock<IConfigurationSettingRepository>();
            _memoryCacheMock = new Mock<IMemoryCache>();

            _service = new ConfigurationSettingService(_repositoryMock.Object, _memoryCacheMock.Object);
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WhenRepositoryIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            IConfigurationSettingRepository repository = null;
            var memoryCache = Mock.Of<IMemoryCache>();

            // Act
            Action act = () => new ConfigurationSettingService(repository, memoryCache);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*repository*");
        }

        [TestMethod]
        public void Constructor_WhenMemoryCacheIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var repository = Mock.Of<IConfigurationSettingRepository>();
            IMemoryCache memoryCache = null;

            // Act
            Action act = () => new ConfigurationSettingService(repository, memoryCache);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*cache*");
        }

        #endregion

        #region Cache Behavior Tests

        [TestMethod]
        public async Task RefreshSettingAsync_ShouldRemoveFromCache()
        {
            // Arrange
            var key = ConfigurationConstants.MaxDomainsPerBook;
            var cacheKey = $"Config_{key}";

            _memoryCacheMock.Setup(m => m.Remove(cacheKey))
                .Verifiable();

            // Act
            await _service.RefreshSettingAsync(key);

            // Assert
            _memoryCacheMock.Verify(m => m.Remove(cacheKey), Times.Once);
        }

        #endregion

        #region Book Constants Tests

        [TestMethod]
        public async Task GetMaxDomainsPerBookAsync_WhenRepositoryReturnsValue_ShouldReturnRepositoryValue()
        {
            // Arrange
            var expectedValue = 7;
            object cachedValue = null;
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetIntValueAsync(ConfigurationConstants.MaxDomainsPerBook, It.IsAny<int>()))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _service.GetMaxDomainsPerBookAsync();

            // Assert
            result.Should().Be(expectedValue);
        }

        #endregion

        #region Reader Constants Tests

        [TestMethod]
        public async Task GetSameBookDelayDaysAsync_ForRegularReader_ShouldReturnBaseValue()
        {
            // Arrange
            var baseValue = 7;
            object cachedValue = null;
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetIntValueAsync(ConfigurationConstants.SameBookDelayDays, It.IsAny<int>()))
                .ReturnsAsync(baseValue);

            // Act
            var result = await _service.GetSameBookDelayDaysAsync(forLibrarian: false);

            // Assert
            result.Should().Be(baseValue);
        }

        [TestMethod]
        public async Task GetBorrowingPeriodDaysAsync_ShouldReturnRepositoryValue()
        {
            // Arrange
            var expectedValue = 30;
            object cachedValue = null;
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetIntValueAsync(ConfigurationConstants.BorrowingPeriodDays, It.IsAny<int>()))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _service.GetBorrowingPeriodDaysAsync();

            // Assert
            result.Should().Be(expectedValue);
        }

        [TestMethod]
        public async Task GetMaxBooksInPeriodAsync_ForRegularReader_ShouldReturnBaseValue()
        {
            // Arrange
            var baseValue = 15;
            object cachedValue = null;
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetIntValueAsync(ConfigurationConstants.MaxBooksInPeriod, It.IsAny<int>()))
                .ReturnsAsync(baseValue);

            // Act
            var result = await _service.GetMaxBooksInPeriodAsync(forLibrarian: false);

            // Assert
            result.Should().Be(baseValue);
        }

        [TestMethod]
        public async Task GetMaxBooksInPeriodAsync_ForLibrarian_ShouldReturnDoubleBaseValue()
        {
            // Arrange
            var baseValue = 15;
            object cachedValue = null;
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetIntValueAsync(ConfigurationConstants.MaxBooksInPeriod, It.IsAny<int>()))
                .ReturnsAsync(baseValue);

            // Act
            var result = await _service.GetMaxBooksInPeriodAsync(forLibrarian: true);

            // Assert
            result.Should().Be(baseValue * 2);
        }

        [TestMethod]
        public async Task GetMaxBooksInPeriodWindowDaysAsync_ForRegularReader_ShouldReturnBaseValue()
        {
            // Arrange
            var baseValue = 20;
            object cachedValue = null;
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetIntValueAsync(ConfigurationConstants.MaxBooksInPeriodWindowDays, It.IsAny<int>()))
                .ReturnsAsync(baseValue);

            // Act
            var result = await _service.GetMaxBooksInPeriodWindowDaysAsync(forLibrarian: false);

            // Assert
            result.Should().Be(baseValue);
        }

        [TestMethod]
        public async Task GetMaxBooksInPeriodWindowDaysAsync_ForLibrarian_ShouldReturnHalfBaseValue()
        {
            // Arrange
            var baseValue = 20;
            object cachedValue = null;
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetIntValueAsync(ConfigurationConstants.MaxBooksInPeriodWindowDays, It.IsAny<int>()))
                .ReturnsAsync(baseValue);

            // Act
            var result = await _service.GetMaxBooksInPeriodWindowDaysAsync(forLibrarian: true);

            // Assert
            result.Should().Be(baseValue / 2);
        }

        [TestMethod]
        public async Task GetMaxBooksPerBorrowingAsync_ForRegularReader_ShouldReturnBaseValue()
        {
            // Arrange
            var baseValue = 5;
            object cachedValue = null;
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetIntValueAsync(ConfigurationConstants.MaxBooksPerBorrowing, It.IsAny<int>()))
                .ReturnsAsync(baseValue);

            // Act
            var result = await _service.GetMaxBooksPerBorrowingAsync(forLibrarian: false);

            // Assert
            result.Should().Be(baseValue);
        }

        [TestMethod]
        public async Task GetMaxBooksPerBorrowingAsync_ForLibrarian_ShouldReturnDoubleBaseValue()
        {
            // Arrange
            var baseValue = 5;
            object cachedValue = null;
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetIntValueAsync(ConfigurationConstants.MaxBooksPerBorrowing, It.IsAny<int>()))
                .ReturnsAsync(baseValue);

            // Act
            var result = await _service.GetMaxBooksPerBorrowingAsync(forLibrarian: true);

            // Assert
            result.Should().Be(baseValue * 2);
        }

        [TestMethod]
        public async Task GetMaxBooksSameDomainAsync_ForRegularReader_ShouldReturnBaseValue()
        {
            // Arrange
            var baseValue = 3;
            object cachedValue = null;
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetIntValueAsync(ConfigurationConstants.MaxBooksSameDomain, It.IsAny<int>()))
                .ReturnsAsync(baseValue);

            // Act
            var result = await _service.GetMaxBooksSameDomainAsync(forLibrarian: false);

            // Assert
            result.Should().Be(baseValue);
        }

        [TestMethod]
        public async Task GetSameDomainTimeLimitMonthsAsync_ShouldReturnRepositoryValue()
        {
            // Arrange
            var expectedValue = 4;
            object cachedValue = null;
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetIntValueAsync(ConfigurationConstants.SameDomainTimeLimitMonths, It.IsAny<int>()))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _service.GetSameDomainTimeLimitMonthsAsync();

            // Assert
            result.Should().Be(expectedValue);
        }

        [TestMethod]
        public async Task GetMaxOvertimeSumDaysAsync_ForRegularReader_ShouldReturnBaseValue()
        {
            // Arrange
            var baseValue = 14;
            object cachedValue = null;
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetIntValueAsync(ConfigurationConstants.MaxOvertimeSumDays, It.IsAny<int>()))
                .ReturnsAsync(baseValue);

            // Act
            var result = await _service.GetMaxOvertimeSumDaysAsync(forLibrarian: false);

            // Assert
            result.Should().Be(baseValue);
        }

        [TestMethod]
        public async Task GetExtensionWindowMonthsAsync_ShouldReturnRepositoryValue()
        {
            // Arrange
            var expectedValue = 3;
            object cachedValue = null;
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetIntValueAsync(ConfigurationConstants.ExtensionWindowMonths, It.IsAny<int>()))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _service.GetExtensionWindowMonthsAsync();

            // Assert
            result.Should().Be(expectedValue);
        }

        [TestMethod]
        public async Task GetMaxBooksPerDayAsync_ShouldReturnRepositoryValue()
        {
            // Arrange
            var expectedValue = 10;
            object cachedValue = null;
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetIntValueAsync(ConfigurationConstants.MaxBooksPerDay, It.IsAny<int>()))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _service.GetMaxBooksPerDayAsync();

            // Assert
            result.Should().Be(expectedValue);
        }

        #endregion

        #region Librarian Constants Tests

        [TestMethod]
        public async Task GetMaxBooksLentPerDayAsync_ShouldReturnRepositoryValue()
        {
            // Arrange
            var expectedValue = 50;
            object cachedValue = null;
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetIntValueAsync(ConfigurationConstants.MaxBooksLentPerDay, It.IsAny<int>()))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _service.GetMaxBooksLentPerDayAsync();

            // Assert
            result.Should().Be(expectedValue);
        }

        #endregion
    }
}
