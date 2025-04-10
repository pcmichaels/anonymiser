using System;
using System.Threading.Tasks;
using System.Xml;
using Anonymiser.Interfaces;
using Anonymiser.Models.Configuration;

namespace Anonymiser.Strategies
{
    public class XmlAnonymisationStrategy : IXmlAnonymisationStrategy
    {
        private readonly IAnonymisationStrategy _valueAnonymisationStrategy;

        public XmlAnonymisationStrategy(IAnonymisationStrategy valueAnonymisationStrategy)
        {
            _valueAnonymisationStrategy = valueAnonymisationStrategy ?? throw new ArgumentNullException(nameof(valueAnonymisationStrategy));
        }

        public string Anonymise(string value, bool isConsistent)
        {
            return _valueAnonymisationStrategy.Anonymise(value, isConsistent);
        }

        public Task<string> AnonymiseXmlAsync(string xmlContent, AnonymisationConfig config)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);
            var root = xmlDoc.DocumentElement;
            if (root != null)
            {
                AnonymiseXmlElement(root, config);
            }
            return Task.FromResult(xmlDoc.OuterXml);
        }

        private void AnonymiseXmlElement(XmlElement element, AnonymisationConfig config)
        {
            foreach (XmlNode child in element.ChildNodes)
            {
                if (child is XmlElement childElement)
                {
                    AnonymiseXmlElement(childElement, config);
                }
                else if (child is XmlText textNode && textNode.Value != null)
                {
                    var propertyConfig = config.PropertiesToAnonymise.Find(p => 
                        p.PropertyName.Equals(element.Name, StringComparison.OrdinalIgnoreCase));

                    if (propertyConfig != null)
                    {
                        textNode.Value = _valueAnonymisationStrategy.Anonymise(
                            textNode.Value,
                            propertyConfig.IsConsistent);
                    }
                }
            }
        }
    }
} 