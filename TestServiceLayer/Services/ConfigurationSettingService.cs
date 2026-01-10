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

        [TestMethod]
        public void Constructor_ShouldInitializeDefaultValues()
        {
            // Arrange & Act
            var service = new ConfigurationSettingService(_repositoryMock.Object, _memoryCacheMock.Object);

            // Assert
            var defaultValuesField = typeof(ConfigurationSettingService)
                .GetField("_defaultValues", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var defaultValues = defaultValuesField?.GetValue(service) as ConcurrentDictionary<string, object>;

            defaultValues.Should().NotBeNull();
            defaultValues.Should().ContainKey(ConfigurationConstants.MaxDomainsPerBook);
            defaultValues[ConfigurationConstants.MaxDomainsPerBook].Should().Be(ConfigurationConstants.DefaultMaxDomainsPerBook);
        }

        #endregion

        #region Cache Behavior Tests

        [TestMethod]
        public async Task GetCachedValueAsync_WhenValueNotInCache_ShouldCallRepositoryAndCacheResult()
        {
            // Arrange
            var key = ConfigurationConstants.MaxDomainsPerBook;
            var expectedValue = 10;
            object cachedValue = null;
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue($"Config_{key}", out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry($"Config_{key}"))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetIntValueAsync(key, It.IsAny<int>()))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _service.GetMaxDomainsPerBookAsync();

            // Assert
            result.Should().Be(expectedValue);
            _repositoryMock.Verify(r => r.GetIntValueAsync(key, It.IsAny<int>()), Times.Once);
        }

        [TestMethod]
        public async Task GetCachedValueAsync_WhenValueInCache_ShouldNotCallRepository()
        {
            // Arrange
            var key = ConfigurationConstants.MaxDomainsPerBook;
            var cachedValue = 15;
            object outValue = cachedValue;

            _memoryCacheMock.Setup(m => m.TryGetValue($"Config_{key}", out outValue))
                .Returns(true);

            // Act
            var result = await _service.GetMaxDomainsPerBookAsync();

            // Assert
            result.Should().Be(cachedValue);
            _repositoryMock.Verify(r => r.GetIntValueAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

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

        [TestMethod]
        public async Task GetMaxDomainsPerBookAsync_WhenRepositoryReturnsDefault_ShouldReturnDefault()
        {
            // Arrange
            object cachedValue = null;
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetIntValueAsync(ConfigurationConstants.MaxDomainsPerBook, It.IsAny<int>()))
                .ReturnsAsync(ConfigurationConstants.DefaultMaxDomainsPerBook);

            // Act
            var result = await _service.GetMaxDomainsPerBookAsync();

            // Assert
            result.Should().Be(ConfigurationConstants.DefaultMaxDomainsPerBook);
        }

        [TestMethod]
        public async Task GetDefaultMaxDomainsPerBookAsync_ShouldReturnConstantDefault()
        {
            // Act
            var result = await _service.GetDefaultMaxDomainsPerBookAsync();

            // Assert
            result.Should().Be(ConfigurationConstants.DefaultMaxDomainsPerBook);
        }

        #endregion

        #region Reader Constants Tests

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
        public async Task GetBorrowingPeriodAsync_ShouldReturnTimeSpanFromDays()
        {
            // Arrange
            double days = 30.0;
            object cachedValue = null;

            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetDoubleValueAsync(
                    ConfigurationConstants.BorrowingPeriodDays, 
                    It.IsAny<double>()))
                .ReturnsAsync(days);

            // Act
            var result = await _service.GetBorrowingPeriodAsync();

            // Assert
            result.Should().Be(TimeSpan.FromDays(days));
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
        public async Task GetMaxBooksSameDomainAsync_ForLibrarian_ShouldReturnDoubleBaseValue()
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
            var result = await _service.GetMaxBooksSameDomainAsync(forLibrarian: true);

            // Assert
            result.Should().Be(baseValue * 2);
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
        public async Task GetMaxOvertimeSumDaysAsync_ForLibrarian_ShouldReturnDoubleBaseValue()
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
            var result = await _service.GetMaxOvertimeSumDaysAsync(forLibrarian: true);

            // Assert
            result.Should().Be(baseValue * 2);
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
        public async Task GetSameBookDelayAsync_ForRegularReader_ShouldReturnBaseTimeSpan()
        {
            // Arrange
            var days = 7.0;
            object cachedValue = null;
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetDoubleValueAsync(ConfigurationConstants.SameBookDelayDays, It.IsAny<double>()))
                .ReturnsAsync(days);

            // Act
            var result = await _service.GetSameBookDelayAsync(forLibrarian: false);

            // Assert
            result.Should().Be(TimeSpan.FromDays(days));
        }

        [TestMethod]
        public async Task GetSameBookDelayAsync_ForLibrarian_ShouldReturnHalfTimeSpan()
        {
            // Arrange
            var days = 7.0;
            object cachedValue = null;
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetDoubleValueAsync(ConfigurationConstants.SameBookDelayDays, It.IsAny<double>()))
                .ReturnsAsync(days);

            // Act
            var result = await _service.GetSameBookDelayAsync(forLibrarian: true);

            // Assert
            result.Should().Be(TimeSpan.FromDays(days / 2));
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

        #region Edge Cases and Boundary Tests

        [TestMethod]
        public async Task GetMaxBooksInPeriodAsync_WithOddNumberForLibrarian_ShouldHandleIntegerDivisionCorrectly()
        {
            // Arrange
            var baseValue = 11;
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
            result.Should().Be(22);
        }

        [TestMethod]
        public async Task GetMaxBooksInPeriodWindowDaysAsync_WithOddNumberForLibrarian_ShouldHandleIntegerDivisionCorrectly()
        {
            // Arrange
            var baseValue = 15;
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
            result.Should().Be(7);
        }

        [TestMethod]
        public async Task GetSameBookDelayAsync_WithDecimalDaysForLibrarian_ShouldReturnHalfDays()
        {
            // Arrange
            var days = 7.5;
            object cachedValue = null;
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetDoubleValueAsync(ConfigurationConstants.SameBookDelayDays, It.IsAny<double>()))
                .ReturnsAsync(days);

            // Act
            var result = await _service.GetSameBookDelayAsync(forLibrarian: true);

            // Assert
            result.Should().Be(TimeSpan.FromDays(3.75));
        }

        [TestMethod]
        public async Task RefreshSettingAsync_WithNullKey_ShouldNotThrow()
        {
            // Arrange
            _memoryCacheMock.Setup(m => m.Remove(It.IsAny<object>()))
                .Verifiable();

            // Act
            Func<Task> act = async () => await _service.RefreshSettingAsync(null);

            // Assert
            await act.Should().NotThrowAsync();
            _memoryCacheMock.Verify(m => m.Remove(It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        public async Task GetCachedValueAsync_WhenRepositoryThrowsException_ShouldThrow()
        {
            // Arrange
            object cachedValue = null;
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);
            _memoryCacheMock.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            _repositoryMock.Setup(r => r.GetIntValueAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act
            Func<Task> act = async () => await _service.GetMaxDomainsPerBookAsync();

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Database error");
        }

        #endregion

        #region ConcurrentDictionary Default Values Tests

        [TestMethod]
        public void Constructor_ShouldInitializeAllDefaultValuesFromConstants()
        {
            // Arrange & Act
            var service = new ConfigurationSettingService(_repositoryMock.Object, _memoryCacheMock.Object);

            var defaultValuesField = typeof(ConfigurationSettingService)
                .GetField("_defaultValues", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var defaultValues = defaultValuesField?.GetValue(service) as ConcurrentDictionary<string, object>;

            // Assert
            defaultValues.Should().NotBeNull();

            defaultValues.Should().ContainKey(ConfigurationConstants.MaxDomainsPerBook);
            defaultValues[ConfigurationConstants.MaxDomainsPerBook].Should().Be(ConfigurationConstants.DefaultMaxDomainsPerBook);

            defaultValues.Should().ContainKey(ConfigurationConstants.MaxBooksInPeriod);
            defaultValues[ConfigurationConstants.MaxBooksInPeriod].Should().Be(ConfigurationConstants.DefaultMaxBooksInPeriod);

            defaultValues.Should().ContainKey(ConfigurationConstants.MaxBooksLentPerDay);
            defaultValues[ConfigurationConstants.MaxBooksLentPerDay].Should().Be(ConfigurationConstants.DefaultMaxBooksLentPerDay);
        }

        #endregion
    }
}
