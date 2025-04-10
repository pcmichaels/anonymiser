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
        private readonly string _seed;

        public MaskingAnonymisationStrategy(string seed)
        {
            _consistentMappings = new Dictionary<string, string>();
            _seed = seed ?? string.Empty;
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

                var newMaskedValue = GenerateConsistentMaskedValue(value);
                _consistentMappings[value] = newMaskedValue;
                return newMaskedValue;
            }

            return GenerateNonConsistentMaskedValue(value);
        }

        private string GenerateConsistentMaskedValue(string value)
        {
            var length = value.Length;
            var result = new StringBuilder(length);

            // Use the seed + value to generate a deterministic hash
            var seedBytes = Encoding.UTF8.GetBytes(_seed + value);
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(seedBytes);

            for (int i = 0; i < length; i++)
            {
                if (char.IsLetter(value[i]))
                {
                    result.Append((char)('a' + (hash[i] % 26)));
                }
                else if (char.IsDigit(value[i]))
                {
                    result.Append((char)('0' + (hash[i] % 10)));
                }
                else
                {
                    result.Append(value[i]); // Preserve special characters
                }
            }

            return result.ToString();
        }

        private string GenerateNonConsistentMaskedValue(string value)
        {
            var length = value.Length;
            var result = new StringBuilder(length);
            var bytes = new byte[length];

            // Generate new random bytes each time
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            for (int i = 0; i < length; i++)
            {
                if (char.IsLetter(value[i]))
                {
                    result.Append((char)('a' + (bytes[i] % 26)));
                }
                else if (char.IsDigit(value[i]))
                {
                    result.Append((char)('0' + (bytes[i] % 10)));
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