using System;
using System.Threading.Tasks;
using System.Xml;
using Anonymiser.Interfaces;
using Anonymiser.Models;

namespace Anonymiser.Strategies
{
    public class XmlParserStrategy : IParserStrategy
    {
        public bool CanParse(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return false;

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(content);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Task<ParserResult<object>> ParseAsync(string content)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(content);
                return Task.FromResult(ParserResult<object>.Success(doc));
            }
            catch (Exception ex)
            {
                return Task.FromResult(ParserResult<object>.Failure($"Failed to parse XML: {ex.Message}"));
            }
        }
    }
} 