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
                ""email"": ""test@example.com"",
                ""name"": ""John Doe"",
                ""address"": ""123 Main St"",
                ""sex"": ""Male"",
                ""gender"": ""Male""
            }";

            // Act
            var result1 = await _anonymiser.AnonymiseJsonAsync(inputJson);
            var result2 = await _anonymiser.AnonymiseJsonAsync(inputJson);

            // Assert
            var anonymised1 = JsonDocument.Parse(result1).RootElement;
            var anonymised2 = JsonDocument.Parse(result2).RootElement;

            // Consistent fields should be the same
            Assert.Equal(anonymised1.GetProperty("email").GetString(), 
                        anonymised2.GetProperty("email").GetString());
            Assert.Equal(anonymised1.GetProperty("name").GetString(), 
                        anonymised2.GetProperty("name").GetString());

            // Non-consistent fields should be different
            Assert.NotEqual(anonymised1.GetProperty("address").GetString(), 
                          anonymised2.GetProperty("address").GetString());
            Assert.NotEqual(anonymised1.GetProperty("sex").GetString(), 
                          anonymised2.GetProperty("sex").GetString());
            Assert.NotEqual(anonymised1.GetProperty("gender").GetString(), 
                          anonymised2.GetProperty("gender").GetString());
        }

        [Fact]
        public async Task AnonymiseJson_WithOverrideConfig_ShouldUseOverride()
        {
            // Arrange
            var inputJson = @"{
                ""email"": ""test@example.com"",
                ""name"": ""John Doe""
            }";

            var overrideConfig = new AnonymisationConfig
            {
                DataLocation = ".",
                AnonymiseSeed = "override-seed",
                PropertiesToAnonymise = new System.Collections.Generic.List<PropertyToAnonymise>
                {
                    new PropertyToAnonymise 
                    { 
                        PropertyName = "email", 
                        AnonymisationType = "Mask", 
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
            Assert.NotEqual(anonymised1.GetProperty("email").GetString(), 
                          anonymised2.GetProperty("email").GetString());

            // Name should be preserved as it's not in override config
            Assert.Equal("John Doe", anonymised2.GetProperty("name").GetString());
        }
    }
} 