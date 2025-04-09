using Anonymiser.Models.Configuration;

namespace Anonymiser.Interfaces
{
    public interface IAnonymisationStrategy
    {
        string Anonymise(string value, bool isConsistent);
    }

    public interface IJsonAnonymisationStrategy : IAnonymisationStrategy
    {
        Task<string> AnonymiseJsonAsync(string jsonContent, AnonymisationConfig config);
    }

    public interface IXmlAnonymisationStrategy : IAnonymisationStrategy
    {
        Task<string> AnonymiseXmlAsync(string xmlContent, AnonymisationConfig config);
    }
} 