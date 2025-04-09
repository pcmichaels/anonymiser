using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Anonymiser;
using Anonymiser.Models.Configuration;
using Xunit;

namespace Anonymiser.Tests
{
    public class AnonymiserTests
    {
        private readonly string _testConfigPath;
        private readonly Anonymiser _anonymiser;

        public AnonymiserTests()
        {
            _testConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "test-config.json");
            _anonymiser = new Anonymiser(_testConfigPath);
        }

        [Fact]
        public async Task AnonymiseJson_WithConsistentFields_ShouldProduceSameOutput()
        {
            // Arrange
            var inputJson = @"{
                ""Email"": ""test@example.com"",
                ""Name"": ""John Doe"",
                ""Address"": ""123 Main St"",
                ""Sex"": ""Male"",
                ""Gender"": ""Male""
            }";

            // Act
            var result1 = await _anonymiser.AnonymiseJsonAsync(inputJson);
            var result2 = await _anonymiser.AnonymiseJsonAsync(inputJson);

            // Assert
            var anonymised1 = JsonDocument.Parse(result1).RootElement;
            var anonymised2 = JsonDocument.Parse(result2).RootElement;

            // Consistent fields should be the same
            Assert.Equal(anonymised1.GetProperty("Email").GetString(), 
                        anonymised2.GetProperty("Email").GetString());
            Assert.Equal(anonymised1.GetProperty("Name").GetString(), 
                        anonymised2.GetProperty("Name").GetString());

            // Non-consistent fields should be different
            Assert.NotEqual(anonymised1.GetProperty("Address").GetString(), 
                          anonymised2.GetProperty("Address").GetString());
            Assert.NotEqual(anonymised1.GetProperty("Sex").GetString(), 
                          anonymised2.GetProperty("Sex").GetString());
            Assert.NotEqual(anonymised1.GetProperty("Gender").GetString(), 
                          anonymised2.GetProperty("Gender").GetString());
        }

        [Fact]
        public async Task AnonymiseJson_WithOverrideConfig_ShouldUseOverride()
        {
            // Arrange
            var inputJson = @"{
                ""Email"": ""test@example.com"",
                ""Name"": ""John Doe""
            }";

            var overrideConfig = new AnonymisationConfig
            {
                AnonymiseSeed = "override-seed",
                PropertiesToAnonymise = new System.Collections.Generic.List<PropertyToAnonymise>
                {
                    new PropertyToAnonymise 
                    { 
                        PropertyName = "Email", 
                        AnonymisationType = "mask", 
                        IsConsistent = true 
                    }
                }
            };

            // Act
            var result1 = await _anonymiser.AnonymiseJsonAsync(inputJson);
            var result2 = await _anonymiser.AnonymiseJsonAsync(inputJson, overrideConfig);

            // Assert
            var anonymised1 = JsonDocument.Parse(result1).RootElement;
            var anonymised2 = JsonDocument.Parse(result2).RootElement;

            // Should be different due to different seed
            Assert.NotEqual(anonymised1.GetProperty("Email").GetString(), 
                          anonymised2.GetProperty("Email").GetString());

            // Name should be preserved as it's not in override config
            Assert.Equal("John Doe", anonymised2.GetProperty("Name").GetString());
        }
    }
} 