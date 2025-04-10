using System.Collections.Generic;

namespace Anonymiser.Models.Configuration
{
    public class AnonymisationConfig
    {
        public required string DataLocation { get; set; }
        public required string AnonymiseSeed { get; set; }
        public required List<PropertyToAnonymise> PropertiesToAnonymise { get; set; }
    }

    public class PropertyToAnonymise
    {
        public required string PropertyName { get; set; }
        public required string AnonymisationType { get; set; }
        public bool IsConsistent { get; set; }
    }
} 