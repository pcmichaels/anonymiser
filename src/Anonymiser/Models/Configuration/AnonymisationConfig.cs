using System.Collections.Generic;

namespace Anonymiser.Models.Configuration
{
    public class AnonymisationConfig
    {
        public string DataLocation { get; set; }
        public string AnonymiseSeed { get; set; }
        public List<PropertyToAnonymise> PropertiesToAnonymise { get; set; }
    }

    public class PropertyToAnonymise
    {
        public string PropertyName { get; set; }
        public string AnonymisationType { get; set; }
        public bool IsConsistent { get; set; }
    }
} 