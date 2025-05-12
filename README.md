# Anonymiser

A .NET library for anonymizing sensitive data in JSON, XML, and CSV documents while maintaining consistency across multiple files.

## Features

- Anonymize sensitive data in JSON, XML, and CSV documents
- Maintain consistent anonymization for specified fields across multiple files
- Configurable anonymization rules
- Support for different anonymization strategies
- Asynchronous processing

## Installation

```powershell
dotnet add package Anonymiser
```

## Usage

### Basic Usage

```csharp
using Anonymiser;
using Anonymiser.Models.Configuration;

// Initialize with configuration file
var anonymiser = new Anonymiser("config.json");

// Or initialize with configuration object
var config = new AnonymisationConfig
{
    AnonymiseSeed = "your-seed",
    PropertiesToAnonymise = new List<PropertyToAnonymise>
    {
        new PropertyToAnonymise 
        { 
            PropertyName = "Email", 
            AnonymisationType = "mask", 
            IsConsistent = true 
        }
    }
};
var anonymiser = new Anonymiser(config);

// Anonymize JSON
var anonymisedJson = await anonymiser.AnonymiseJsonAsync(jsonContent);

// Anonymize XML
var anonymisedXml = await anonymiser.AnonymiseXmlAsync(xmlContent);

// Anonymize CSV
var anonymisedCsv = await anonymiser.AnonymiseCsvAsync(csvContent);
```

### Configuration

The configuration file (`config.json`) should follow this structure:

```json
{
    "AnonymiseSeed": "your-seed",
    "PropertiesToAnonymise": [
        {
            "PropertyName": "Email",
            "AnonymisationType": "mask",
            "IsConsistent": true
        },
        {
            "PropertyName": "Name",
            "AnonymisationType": "mask",
            "IsConsistent": true
        }
    ]
}
```

- `AnonymiseSeed`: A string used to seed the anonymization process for consistent results
- `PropertiesToAnonymise`: List of properties to anonymize
  - `PropertyName`: The name of the property to anonymize
  - `AnonymisationType`: The type of anonymization to apply (currently only "mask" is supported)
  - `IsConsistent`: Whether the same input should always produce the same anonymized output

### Consistent Anonymization

When `IsConsistent` is set to `true` for a property, the same input value will always produce the same anonymized output, even across different files. This is useful for maintaining referential integrity in your data.

## Examples

### JSON Anonymization

Input:
```json
{
    "Name": "John Doe",
    "Email": "john.doe@example.com",
    "customerName": "Jane Smith",
    "customerEmail": "jane.smith@example.com"
}
```

Output (example):
```json
{
    "Name": "abc123",
    "Email": "xyz789",
    "customerName": "def456",
    "customerEmail": "uvw012"
}
```

### XML Anonymization

Input:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<Root>
    <Name>John Doe</Name>
    <Email>john.doe@example.com</Email>
    <customerName>Jane Smith</customerName>
    <customerEmail>jane.smith@example.com</customerEmail>
</Root>
```

Output (example):
```xml
<?xml version="1.0" encoding="UTF-8"?>
<Root>
    <Name>abc123</Name>
    <Email>xyz789</Email>
    <customerName>def456</customerName>
    <customerEmail>uvw012</customerEmail>
</Root>
```

### CSV Anonymization

Input:
```csv
Name,Email,Phone,Address
John Doe,john.doe@example.com,123-456-7890,123 Main St
Jane Smith,jane.smith@example.com,987-654-3210,456 Oak Ave
```

Output (example):
```csv
Name,Email,Phone,Address
abc123,xyz789,def456,uvw012
ghi789,jkl012,mno345,pqr678
```

Note: The CSV anonymization will:
- Preserve the header row
- Anonymize values in specified columns
- Maintain consistent anonymization across multiple files
- Handle quoted values and special characters correctly

## License

This project is licensed under the MIT License - see the LICENSE file for details. 