using System;
using System.Collections.Generic;
using System.Text;
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
            if (root == null)
            {
                throw new InvalidOperationException("XML document has no root element");
            }

            AnonymiseXmlElement(root, config);

            var sb = new StringBuilder();
            using var writer = new XmlTextWriter(new StringWriter(sb));
            xmlDoc.WriteTo(writer);
            return Task.FromResult(sb.ToString());
        }

        private void AnonymiseXmlElement(XmlElement element, AnonymisationConfig config)
        {
            // Check if this element should be anonymized - using exact case-sensitive match
            var propertyConfig = config.PropertiesToAnonymise.Find(p => 
                p.PropertyName == element.Name);

            // If this element should be anonymized, process its text nodes
            if (propertyConfig != null)
            {
                foreach (XmlNode child in element.ChildNodes)
                {
                    if (child is XmlText textNode && textNode.Value != null)
                    {
                        textNode.Value = _valueAnonymisationStrategy.Anonymise(
                            textNode.Value,
                            propertyConfig.IsConsistent);
                    }
                }
            }
            else
            {
                // If this element should not be anonymized, process its child elements
                foreach (XmlNode child in element.ChildNodes)
                {
                    if (child is XmlElement childElement)
                    {
                        AnonymiseXmlElement(childElement, config);
                    }
                }
            }
        }
    }
} 