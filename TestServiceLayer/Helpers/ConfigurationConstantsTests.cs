using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLayer.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServiceLayer.Helpers
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ConfigurationConstantsTests
    {

        [TestMethod]
        public void GetAllDefaults_ShouldReturnCorrectNumberOfItems()
        {
            // Act
            var defaults = ConfigurationConstants.GetAllDefaults();

            // Assert
            defaults.Should().HaveCount(12);
        }
    }
}
