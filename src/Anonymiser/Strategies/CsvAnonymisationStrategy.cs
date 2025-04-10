using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anonymiser.Interfaces;
using Anonymiser.Models.Configuration;

namespace Anonymiser.Strategies
{
    public class CsvAnonymisationStrategy : ICsvAnonymisationStrategy
    {
        private readonly IAnonymisationStrategy _valueAnonymisationStrategy;

        public CsvAnonymisationStrategy(IAnonymisationStrategy valueAnonymisationStrategy)
        {
            _valueAnonymisationStrategy = valueAnonymisationStrategy ?? throw new ArgumentNullException(nameof(valueAnonymisationStrategy));
        }

        public string Anonymise(string value, bool isConsistent)
        {
            return _valueAnonymisationStrategy.Anonymise(value, isConsistent);
        }

        public Task<string> AnonymiseCsvAsync(string csvContent, AnonymisationConfig config)
        {
            if (string.IsNullOrWhiteSpace(csvContent))
            {
                throw new ArgumentException("CSV content cannot be empty", nameof(csvContent));
            }

            var lines = csvContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
            {
                throw new ArgumentException("CSV must contain at least a header row and one data row", nameof(csvContent));
            }

            var headers = lines[0].Split(',');
            var anonymisedLines = new List<string> { lines[0] }; // Keep original header

            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(',');
                if (values.Length != headers.Length)
                {
                    throw new ArgumentException($"Row {i + 1} has {values.Length} columns but header has {headers.Length} columns", nameof(csvContent));
                }

                var anonymisedValues = new string[values.Length];
                for (int j = 0; j < values.Length; j++)
                {
                    var header = headers[j].Trim();
                    var propertyConfig = config.PropertiesToAnonymise.Find(p => 
                        p.PropertyName.Equals(header, StringComparison.OrdinalIgnoreCase));

                    if (propertyConfig != null)
                    {
                        anonymisedValues[j] = _valueAnonymisationStrategy.Anonymise(values[j].Trim(), propertyConfig.IsConsistent);
                    }
                    else
                    {
                        anonymisedValues[j] = values[j];
                    }
                }

                anonymisedLines.Add(string.Join(",", anonymisedValues));
            }

            return Task.FromResult(string.Join(Environment.NewLine, anonymisedLines));
        }
    }
} 