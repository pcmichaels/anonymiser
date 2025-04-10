using System;
using System.Threading.Tasks;
using Anonymiser.Models.Configuration;
using Anonymiser.Strategies;
using Xunit;

namespace Anonymiser.Tests.Strategies
{
    public class CsvAnonymisationStrategyTests
    {
        private readonly CsvAnonymisationStrategy _strategy;
        private readonly AnonymisationConfig _config;

        public CsvAnonymisationStrategyTests()
        {
            var valueStrategy = new MaskingAnonymisationStrategy("test-seed");
            _strategy = new CsvAnonymisationStrategy(valueStrategy);

            _config = new AnonymisationConfig
            {
                DataLocation = ".",
                AnonymiseSeed = "test-seed",
                PropertiesToAnonymise = new List<PropertyToAnonymise>
                {
                    new PropertyToAnonymise { PropertyName = "Name", AnonymisationType = "Mask", IsConsistent = true },
                    new PropertyToAnonymise { PropertyName = "Email", AnonymisationType = "Mask", IsConsistent = true },
                    new PropertyToAnonymise { PropertyName = "Customer Address", AnonymisationType = "Mask", IsConsistent = true }
                }
            };
        }

        [Fact]
        public async Task AnonymiseCsvAsync_WithValidCsv_ReturnsAnonymisedCsv()
        {
            // Arrange
            var csvContent = @"Name,Email,OtherField
John Doe,john.doe@example.com,Some data
Jane Smith,jane.smith@example.com,Other data";

            // Act
            var result = await _strategy.AnonymiseCsvAsync(csvContent, _config);

            // Assert
            var lines = result.Split(Environment.NewLine);
            Assert.Equal(3, lines.Length); // Header + 2 data rows
            Assert.Equal("Name,Email,OtherField", lines[0]); // Header unchanged
            
            // First row anonymised
            var firstRow = lines[1].Split(',');
            Assert.Equal(3, firstRow.Length);
            Assert.NotEqual("John Doe", firstRow[0]);
            Assert.NotEqual("john.doe@example.com", firstRow[1]);
            Assert.Equal("Some data", firstRow[2]);

            // Second row anonymised
            var secondRow = lines[2].Split(',');
            Assert.Equal(3, secondRow.Length);
            Assert.NotEqual("Jane Smith", secondRow[0]);
            Assert.NotEqual("jane.smith@example.com", secondRow[1]);
            Assert.Equal("Other data", secondRow[2]);
        }

        [Fact]
        public async Task AnonymiseCsvAsync_WithSpacesInFieldNames_ReturnsAnonymisedCsv()
        {
            // Arrange
            var csvContent = @"Name,Email,Customer Address,OtherField
John Doe,john.doe@example.com,123 Main St,Some data
Jane Smith,jane.smith@example.com,456 Oak Ave,Other data";

            // Act
            var result = await _strategy.AnonymiseCsvAsync(csvContent, _config);

            // Assert
            var lines = result.Split(Environment.NewLine);
            Assert.Equal(3, lines.Length); // Header + 2 data rows
            Assert.Equal("Name,Email,Customer Address,OtherField", lines[0]); // Header unchanged
            
            // First row anonymised
            var firstRow = lines[1].Split(',');
            Assert.Equal(4, firstRow.Length);
            Assert.NotEqual("John Doe", firstRow[0]);
            Assert.NotEqual("john.doe@example.com", firstRow[1]);
            Assert.NotEqual("123 Main St", firstRow[2]);
            Assert.Equal("Some data", firstRow[3]);

            // Second row anonymised
            var secondRow = lines[2].Split(',');
            Assert.Equal(4, secondRow.Length);
            Assert.NotEqual("Jane Smith", secondRow[0]);
            Assert.NotEqual("jane.smith@example.com", secondRow[1]);
            Assert.NotEqual("456 Oak Ave", secondRow[2]);
            Assert.Equal("Other data", secondRow[3]);
        }

        [Fact]
        public async Task AnonymiseCsvAsync_WithConsistentAnonymisation_ReturnsSameValuesForSameInputs()
        {
            // Arrange
            var csvContent = @"Name,Email,OtherField
John Doe,john.doe@example.com,Some data";

            // Act
            var result1 = await _strategy.AnonymiseCsvAsync(csvContent, _config);
            var result2 = await _strategy.AnonymiseCsvAsync(csvContent, _config);

            // Assert
            var lines1 = result1.Split(Environment.NewLine);
            var lines2 = result2.Split(Environment.NewLine);
            Assert.Equal(lines1[1], lines2[1]); // Same input should produce same output
        }

        [Fact]
        public async Task AnonymiseCsvAsync_WithEmptyContent_ThrowsArgumentException()
        {
            // Arrange
            var csvContent = "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _strategy.AnonymiseCsvAsync(csvContent, _config));
        }

        [Fact]
        public async Task AnonymiseCsvAsync_WithNoDataRows_ThrowsArgumentException()
        {
            // Arrange
            var csvContent = "Name,Email,OtherField";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _strategy.AnonymiseCsvAsync(csvContent, _config));
        }

        [Fact]
        public async Task AnonymiseCsvAsync_WithMismatchedColumns_ThrowsArgumentException()
        {
            // Arrange
            var csvContent = @"Name,Email,OtherField
John Doe,john.doe@example.com";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _strategy.AnonymiseCsvAsync(csvContent, _config));
        }
    }
} 