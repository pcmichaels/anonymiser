using System.Threading.Tasks;
using Anonymiser.Models.Configuration;

namespace Anonymiser.Interfaces
{
    public interface ICsvAnonymisationStrategy : IAnonymisationStrategy
    {
        Task<string> AnonymiseCsvAsync(string csvContent, AnonymisationConfig config);
    }
} 