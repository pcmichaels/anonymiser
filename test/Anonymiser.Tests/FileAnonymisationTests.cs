using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using Anonymiser;
using Anonymiser.Models.Configuration;
using Xunit;

namespace Anonymiser.Tests
{
    public class FileAnonymisationTests
    {
        private readonly string _testConfigPath;
        private readonly string _dataPath;
        private readonly Anonymiser _anonymiser;
        private readonly Dictionary<string, Dictionary<string, string>> _consistentMappings;

        public FileAnonymisationTests()
        {
            _testConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "test-config.json");
            _dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            _anonymiser = new Anonymiser(_testConfigPath);
            _consistentMappings = new Dictionary<string, Dictionary<string, string>>();
        }

        [Fact]
        public async Task AnonymiseJsonFiles_ShouldAnonymiseAllFiles()
        {
            // Arrange
            var jsonFiles = Directory.GetFiles(Path.Combine(_dataPath, "Json"), "*.json");
            var originalValues = new Dictionary<string, HashSet<string>>();

            // Act & Assert
            foreach (var file in jsonFiles)
            {
                var content = await File.ReadAllTextAsync(file);
                var anonymisedContent = await _anonymiser.AnonymiseJsonAsync(content);
                var anonymisedJson = JsonDocument.Parse(anonymisedContent).RootElement;

                // Store original values and verify anonymization
                StoreAndVerifyAnonymisation(content, anonymisedContent, originalValues);
            }
        }

        [Fact]
        public async Task AnonymiseXmlFiles_ShouldAnonymiseAllFiles()
        {
            // Arrange
            var xmlFiles = Directory.GetFiles(Path.Combine(_dataPath, "Xml"), "*.xml");
            var originalValues = new Dictionary<string, HashSet<string>>();

            // Act & Assert
            foreach (var file in xmlFiles)
            {
                var content = await File.ReadAllTextAsync(file);
                var anonymisedContent = await _anonymiser.AnonymiseXmlAsync(content);
                var anonymisedXml = new XmlDocument();
                anonymisedXml.LoadXml(anonymisedContent);

                // Convert XML to JSON for verification
                var jsonContent = ConvertXmlToJson(content);
                var anonymisedJson = ConvertXmlToJson(anonymisedContent);

                // Store original values and verify anonymization
                StoreAndVerifyAnonymisation(jsonContent, anonymisedJson, originalValues);
            }
        }

        private string ConvertXmlToJson(string xmlContent)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);
            var root = xmlDoc.DocumentElement;
            var jsonObj = new Dictionary<string, string>();

            foreach (XmlNode node in root.ChildNodes)
            {
                jsonObj[node.Name] = node.InnerText;
            }

            return JsonSerializer.Serialize(jsonObj);
        }

        private void StoreAndVerifyAnonymisation(string originalContent, string anonymisedContent, Dictionary<string, HashSet<string>> originalValues)
        {
            var originalDoc = JsonDocument.Parse(originalContent);
            var anonymisedDoc = JsonDocument.Parse(anonymisedContent);
            var originalRoot = originalDoc.RootElement;
            var anonymisedRoot = anonymisedDoc.RootElement;

            foreach (var property in originalRoot.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String)
                {
                    var originalValue = property.Value.GetString();
                    var anonymisedValue = anonymisedRoot.GetProperty(property.Name).GetString();

                    // Store original value
                    if (!originalValues.ContainsKey(property.Name))
                    {
                        originalValues[property.Name] = new HashSet<string>();
                    }
                    originalValues[property.Name].Add(originalValue);

                    // Check that original value is not present in anonymized content
                    Assert.DoesNotContain(originalValue, anonymisedValue);

                    // Check for consistent anonymization
                    if (IsConsistentField(property.Name))
                    {
                        VerifyConsistentAnonymization(property.Name, originalValue, anonymisedValue);
                    }
                }
            }
        }

        private void VerifyConsistentAnonymization(string fieldName, string originalValue, string anonymisedValue)
        {
            if (!_consistentMappings.ContainsKey(fieldName))
            {
                _consistentMappings[fieldName] = new Dictionary<string, string>();
            }

            // If we've seen this original value before, verify it gets the same anonymized value
            if (_consistentMappings[fieldName].TryGetValue(originalValue, out var existingAnonymisedValue))
            {
                Assert.Equal(existingAnonymisedValue, anonymisedValue);
            }
            else
            {
                // Store the mapping for future checks
                _consistentMappings[fieldName][originalValue] = anonymisedValue;
            }
        }

        private bool IsConsistentField(string fieldName)
        {
            return fieldName.Equals("Email", StringComparison.OrdinalIgnoreCase) ||
                   fieldName.Equals("Name", StringComparison.OrdinalIgnoreCase) ||
                   fieldName.Equals("customerEmail", StringComparison.OrdinalIgnoreCase) ||
                   fieldName.Equals("customerName", StringComparison.OrdinalIgnoreCase);
        }
    }
} 