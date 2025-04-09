using System.Threading.Tasks;
using Anonymiser.Models;

namespace Anonymiser.Interfaces
{
    public interface IParserStrategy
    {
        Task<ParserResult<object>> ParseAsync(string content);
        bool CanParse(string content);
    }
} 