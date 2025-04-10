using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Anonymiser.Interfaces;
using Anonymiser.Models.Configuration;
using Anonymiser.Strategies;

namespace Anonymiser
{
    public class Anonymiser
    {
        private AnonymisationConfig _config;
        private readonly IJsonAnonymisationStrategy _jsonStrategy;
        private readonly IXmlAnonymisationStrategy _xmlStrategy;
        private readonly ICsvAnonymisationStrategy _csvStrategy;

        public Anonymiser(string configPath)
        {
            var configJson = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<AnonymisationConfig>(configJson);
            if (config == null)
            {
                throw new InvalidOperationException("Failed to deserialize configuration");
            }
            _config = config;
            
            var valueStrategy = new MaskingAnonymisationStrategy(_config.AnonymiseSeed);
            _jsonStrategy = new JsonAnonymisationStrategy(valueStrategy);
            _xmlStrategy = new XmlAnonymisationStrategy(valueStrategy);
            _csvStrategy = new CsvAnonymisationStrategy(valueStrategy);
        }

        public Anonymiser(AnonymisationConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            
            var valueStrategy = new MaskingAnonymisationStrategy(_config.AnonymiseSeed);
            _jsonStrategy = new JsonAnonymisationStrategy(valueStrategy);
            _xmlStrategy = new XmlAnonymisationStrategy(valueStrategy);
            _csvStrategy = new CsvAnonymisationStrategy(valueStrategy);
        }

        public void UpdateConfiguration(AnonymisationConfig newConfig)
        {
            _config = newConfig ?? throw new ArgumentNullException(nameof(newConfig));
        }

        public async Task<string> AnonymiseJsonAsync(string jsonContent, AnonymisationConfig? overrideConfig = null)
        {
            if (overrideConfig != null)
            {
                var overrideValueStrategy = new MaskingAnonymisationStrategy(overrideConfig.AnonymiseSeed);
                var overrideJsonStrategy = new JsonAnonymisationStrategy(overrideValueStrategy);
                return await overrideJsonStrategy.AnonymiseJsonAsync(jsonContent, overrideConfig);
            }
            return await _jsonStrategy.AnonymiseJsonAsync(jsonContent, _config);
        }

        public async Task<string> AnonymiseXmlAsync(string xmlContent, AnonymisationConfig? overrideConfig = null)
        {
            if (overrideConfig != null)
            {
                var overrideValueStrategy = new MaskingAnonymisationStrategy(overrideConfig.AnonymiseSeed);
                var overrideXmlStrategy = new XmlAnonymisationStrategy(overrideValueStrategy);
                return await overrideXmlStrategy.AnonymiseXmlAsync(xmlContent, overrideConfig);
            }
            return await _xmlStrategy.AnonymiseXmlAsync(xmlContent, _config);
        }

        public async Task<string> AnonymiseCsvAsync(string csvContent, AnonymisationConfig? overrideConfig = null)
        {
            if (overrideConfig != null)
            {
                var overrideValueStrategy = new MaskingAnonymisationStrategy(overrideConfig.AnonymiseSeed);
                var overrideCsvStrategy = new CsvAnonymisationStrategy(overrideValueStrategy);
                return await overrideCsvStrategy.AnonymiseCsvAsync(csvContent, overrideConfig);
            }
            return await _csvStrategy.AnonymiseCsvAsync(csvContent, _config);
        }

        public async Task<string> AnonymiseFileAsync(string filePath, AnonymisationConfig? overrideConfig = null)
        {
            var content = await File.ReadAllTextAsync(filePath);
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            
            return extension switch
            {
                ".json" => await AnonymiseJsonAsync(content, overrideConfig),
                ".xml" => await AnonymiseXmlAsync(content, overrideConfig),
                ".csv" => await AnonymiseCsvAsync(content, overrideConfig),
                _ => throw new NotSupportedException($"File extension {extension} is not supported")
            };
        }
    }
} 