using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Anonymiser.Interfaces;

namespace Anonymiser.Strategies
{
    public class MaskingAnonymisationStrategy : IAnonymisationStrategy
    {
        private readonly Dictionary<string, string> _consistentMappings;
        private readonly Random _random;

        public MaskingAnonymisationStrategy(string seed)
        {
            _consistentMappings = new Dictionary<string, string>();
            _random = new Random(seed?.GetHashCode() ?? 0);
        }

        public string Anonymise(string value, bool isConsistent)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (isConsistent)
            {
                if (_consistentMappings.TryGetValue(value, out var maskedValue))
                {
                    return maskedValue;
                }

                var newMaskedValue = GenerateMaskedValue(value);
                _consistentMappings[value] = newMaskedValue;
                return newMaskedValue;
            }

            return GenerateMaskedValue(value);
        }

        private string GenerateMaskedValue(string value)
        {
            // For consistent values, use a hash of the input
            // For non-consistent values, use random characters
            var length = value.Length;
            var result = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                if (char.IsLetter(value[i]))
                {
                    result.Append((char)('a' + _random.Next(26)));
                }
                else if (char.IsDigit(value[i]))
                {
                    result.Append((char)('0' + _random.Next(10)));
                }
                else
                {
                    result.Append(value[i]); // Preserve special characters
                }
            }

            return result.ToString();
        }
    }
} 