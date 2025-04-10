using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Anonymiser.Interfaces;
using Anonymiser.Models.Configuration;

namespace Anonymiser.Strategies
{
    public class JsonAnonymisationStrategy : IJsonAnonymisationStrategy
    {
        private readonly IAnonymisationStrategy _valueAnonymisationStrategy;

        public JsonAnonymisationStrategy(IAnonymisationStrategy valueAnonymisationStrategy)
        {
            _valueAnonymisationStrategy = valueAnonymisationStrategy ?? throw new ArgumentNullException(nameof(valueAnonymisationStrategy));
        }

        public string Anonymise(string value, bool isConsistent)
        {
            return _valueAnonymisationStrategy.Anonymise(value, isConsistent);
        }

        public Task<string> AnonymiseJsonAsync(string jsonContent, AnonymisationConfig config)
        {
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;
            var anonymisedJson = AnonymiseJsonElement(root, config);
            return Task.FromResult(JsonSerializer.Serialize(anonymisedJson));
        }

        private object? AnonymiseJsonElement(JsonElement element, AnonymisationConfig config)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var obj = new Dictionary<string, object?>();
                    foreach (var property in element.EnumerateObject())
                    {
                        var propertyConfig = config.PropertiesToAnonymise.Find(p => 
                            p.PropertyName == property.Name);

                        if (propertyConfig != null && property.Value.ValueKind == JsonValueKind.String)
                        {
                            var stringValue = property.Value.GetString();
                            if (stringValue != null)
                            {
                                obj[property.Name] = _valueAnonymisationStrategy.Anonymise(
                                    stringValue, 
                                    propertyConfig.IsConsistent);
                            }
                            else
                            {
                                obj[property.Name] = null;
                            }
                        }
                        else
                        {
                            obj[property.Name] = AnonymiseJsonElement(property.Value, config);
                        }
                    }
                    return obj;

                case JsonValueKind.Array:
                    var array = new List<object?>();
                    foreach (var item in element.EnumerateArray())
                    {
                        array.Add(AnonymiseJsonElement(item, config));
                    }
                    return array;

                case JsonValueKind.String:
                    return element.GetString();

                case JsonValueKind.Number:
                    return element.GetDecimal();

                case JsonValueKind.True:
                case JsonValueKind.False:
                    return element.GetBoolean();

                case JsonValueKind.Null:
                    return null;

                default:
                    throw new NotSupportedException($"Unsupported JSON value kind: {element.ValueKind}");
            }
        }
    }
} 