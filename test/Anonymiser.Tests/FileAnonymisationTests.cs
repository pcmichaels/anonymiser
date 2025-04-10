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
            if (root == null)
            {
                throw new InvalidOperationException("XML document has no root element");
            }

            var jsonObj = new Dictionary<string, object>();

            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.Name != null)
                {
                    if (node.ChildNodes.Count == 1 && node.FirstChild is XmlText)
                    {
                        // Simple text node
                        jsonObj[node.Name] = node.InnerText;
                    }
                    else
                    {
                        // Complex node with children
                        var childObj = new Dictionary<string, object>();
                        foreach (XmlNode child in node.ChildNodes)
                        {
                            if (child.Name != null)
                            {
                                if (child.ChildNodes.Count == 1 && child.FirstChild is XmlText)
                                {
                                    // Simple text node
                                    childObj[child.Name] = child.InnerText;
                                }
                                else
                                {
                                    // Nested complex node
                                    var nestedObj = new Dictionary<string, object>();
                                    foreach (XmlNode nestedChild in child.ChildNodes)
                                    {
                                        if (nestedChild.Name != null)
                                        {
                                            nestedObj[nestedChild.Name] = nestedChild.InnerText;
                                        }
                                    }
                                    childObj[child.Name] = nestedObj;
                                }
                            }
                        }
                        jsonObj[node.Name] = childObj;
                    }
                }
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

                    if (originalValue != null && anonymisedValue != null)
                    {
                        var propertyConfig = GetPropertyConfig(property.Name);
                        if (propertyConfig != null)
                        {
                            if (!originalValues.ContainsKey(property.Name))
                            {
                                originalValues[property.Name] = new HashSet<string>();
                            }

                            if (originalValues[property.Name].Add(originalValue))
                            {
                                // First time seeing this value for this field
                                VerifyConsistentAnonymization(property.Name, originalValue, anonymisedValue, propertyConfig.IsConsistent);
                            }
                            else
                            {
                                // We've seen this value before, verify consistency
                                var previousAnonymisedValue = _consistentMappings[property.Name][originalValue];
                                Assert.Equal(previousAnonymisedValue, anonymisedValue);
                            }
                        }
                        else
                        {
                            // Field is not in configuration, should remain unchanged
                            Assert.Equal(originalValue, anonymisedValue);
                        }
                    }
                }
                else if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    // Handle nested objects
                    var originalObj = property.Value;
                    var anonymisedObj = anonymisedRoot.GetProperty(property.Name);

                    foreach (var nestedProperty in originalObj.EnumerateObject())
                    {
                        if (nestedProperty.Value.ValueKind == JsonValueKind.String)
                        {
                            var originalValue = nestedProperty.Value.GetString();
                            var anonymisedValue = anonymisedObj.GetProperty(nestedProperty.Name).GetString();

                            if (originalValue != null && anonymisedValue != null)
                            {
                                var propertyConfig = GetPropertyConfig(nestedProperty.Name);
                                if (propertyConfig != null)
                                {
                                    if (!originalValues.ContainsKey(nestedProperty.Name))
                                    {
                                        originalValues[nestedProperty.Name] = new HashSet<string>();
                                    }

                                    if (originalValues[nestedProperty.Name].Add(originalValue))
                                    {
                                        // First time seeing this value for this field
                                        VerifyConsistentAnonymization(nestedProperty.Name, originalValue, anonymisedValue, propertyConfig.IsConsistent);
                                    }
                                    else
                                    {
                                        // We've seen this value before, verify consistency
                                        var previousAnonymisedValue = _consistentMappings[nestedProperty.Name][originalValue];
                                        Assert.Equal(previousAnonymisedValue, anonymisedValue);
                                    }
                                }
                                else
                                {
                                    // Field is not in configuration, should remain unchanged
                                    Assert.Equal(originalValue, anonymisedValue);
                                }
                            }
                        }
                        else if (nestedProperty.Value.ValueKind == JsonValueKind.Object)
                        {
                            // Handle nested nested objects
                            var originalNestedObj = nestedProperty.Value;
                            var anonymisedNestedObj = anonymisedObj.GetProperty(nestedProperty.Name);

                            foreach (var nestedNestedProperty in originalNestedObj.EnumerateObject())
                            {
                                if (nestedNestedProperty.Value.ValueKind == JsonValueKind.String)
                                {
                                    var originalValue = nestedNestedProperty.Value.GetString();
                                    var anonymisedValue = anonymisedNestedObj.GetProperty(nestedNestedProperty.Name).GetString();

                                    if (originalValue != null && anonymisedValue != null)
                                    {
                                        var propertyConfig = GetPropertyConfig(nestedNestedProperty.Name);
                                        if (propertyConfig != null)
                                        {
                                            if (!originalValues.ContainsKey(nestedNestedProperty.Name))
                                            {
                                                originalValues[nestedNestedProperty.Name] = new HashSet<string>();
                                            }

                                            if (originalValues[nestedNestedProperty.Name].Add(originalValue))
                                            {
                                                // First time seeing this value for this field
                                                VerifyConsistentAnonymization(nestedNestedProperty.Name, originalValue, anonymisedValue, propertyConfig.IsConsistent);
                                            }
                                            else
                                            {
                                                // We've seen this value before, verify consistency
                                                var previousAnonymisedValue = _consistentMappings[nestedNestedProperty.Name][originalValue];
                                                Assert.Equal(previousAnonymisedValue, anonymisedValue);
                                            }
                                        }
                                        else
                                        {
                                            // Field is not in configuration, should remain unchanged
                                            Assert.Equal(originalValue, anonymisedValue);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void VerifyConsistentAnonymization(string fieldName, string originalValue, string anonymisedValue, bool isConsistent)
        {
            if (isConsistent)
            {
                if (!_consistentMappings.ContainsKey(fieldName))
                {
                    _consistentMappings[fieldName] = new Dictionary<string, string>();
                }

                _consistentMappings[fieldName][originalValue] = anonymisedValue;
            }
            else
            {
                Assert.NotEqual(originalValue, anonymisedValue);
            }
        }

        private PropertyToAnonymise? GetPropertyConfig(string fieldName)
        {
            var configJson = File.ReadAllText(_testConfigPath);
            var config = JsonSerializer.Deserialize<AnonymisationConfig>(configJson);
            if (config?.PropertiesToAnonymise == null)
            {
                return null;
            }

            return config.PropertiesToAnonymise.FirstOrDefault(p => 
                p.PropertyName == fieldName);
        }
    }
} 