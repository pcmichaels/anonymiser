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
        }

        public Anonymiser(AnonymisationConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            
            var valueStrategy = new MaskingAnonymisationStrategy(_config.AnonymiseSeed);
            _jsonStrategy = new JsonAnonymisationStrategy(valueStrategy);
            _xmlStrategy = new XmlAnonymisationStrategy(valueStrategy);
        }

        public void UpdateConfiguration(AnonymisationConfig newConfig)
        {
            _config = newConfig ?? throw new ArgumentNullException(nameof(newConfig));
        }

        public async Task<string> AnonymiseJsonAsync(string jsonContent, AnonymisationConfig? overrideConfig = null)
        {
            var configToUse = overrideConfig ?? _config;
            return await _jsonStrategy.AnonymiseJsonAsync(jsonContent, configToUse);
        }

        public async Task<string> AnonymiseXmlAsync(string xmlContent, AnonymisationConfig? overrideConfig = null)
        {
            var configToUse = overrideConfig ?? _config;
            return await _xmlStrategy.AnonymiseXmlAsync(xmlContent, configToUse);
        }

        public async Task<string> AnonymiseFileAsync(string filePath, AnonymisationConfig? overrideConfig = null)
        {
            var content = await File.ReadAllTextAsync(filePath);
            return await AnonymiseJsonAsync(content, overrideConfig);
        }
    }
} 