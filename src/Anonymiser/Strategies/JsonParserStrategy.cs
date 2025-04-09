using System;
using System.Text.Json;
using System.Threading.Tasks;
using Anonymiser.Interfaces;
using Anonymiser.Models;

namespace Anonymiser.Strategies
{
    public class JsonParserStrategy : IParserStrategy
    {
        public bool CanParse(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return false;

            try
            {
                JsonDocument.Parse(content);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ParserResult<object>> ParseAsync(string content)
        {
            try
            {
                var document = JsonDocument.Parse(content);
                return ParserResult<object>.Success(document);
            }
            catch (Exception ex)
            {
                return ParserResult<object>.Failure($"Failed to parse JSON: {ex.Message}");
            }
        }
    }
} 