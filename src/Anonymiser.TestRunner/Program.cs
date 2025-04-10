using Anonymiser;
using Anonymiser.Models.Configuration;
using Microsoft.Extensions.Configuration;

namespace Anonymiser.TestRunner;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Anonymiser Test Runner");
            Console.WriteLine("=====================");

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            // Get config path from args or use default
            var configPath = args.Length > 0 ? args[0] : null;
            
            // Initialize anonymiser
            Anonymiser anonymiser;
            if (configPath != null)
            {
                anonymiser = new Anonymiser(configPath);
            }
            else
            {
                var config = configuration.Get<AnonymisationConfig>();
                if (config == null)
                {
                    throw new InvalidOperationException("No configuration found. Please provide a config file or ensure appsettings.json exists.");
                }
                anonymiser = new Anonymiser(config);
            }

            // Test JSON anonymization
            Console.WriteLine("\nTesting JSON anonymization...");
            var jsonContent = """
                {
                    "Name": "John Doe",
                    "Email": "john.doe@example.com",
                    "customerName": "Jane Smith",
                    "customerEmail": "jane.smith@example.com",
                    "OtherField": "Some other data"
                }
                """;

            var anonymisedJson = await anonymiser.AnonymiseJsonAsync(jsonContent);
            Console.WriteLine("Original JSON:");
            Console.WriteLine(jsonContent);
            Console.WriteLine("\nAnonymised JSON:");
            Console.WriteLine(anonymisedJson);

            // Test XML anonymization
            Console.WriteLine("\nTesting XML anonymization...");
            var xmlContent = """
                <?xml version="1.0" encoding="UTF-8"?>
                <Root>
                    <Name>John Doe</Name>
                    <Email>john.doe@example.com</Email>
                    <customerName>Jane Smith</customerName>
                    <customerEmail>jane.smith@example.com</customerEmail>
                    <OtherField>Some other data</OtherField>
                </Root>
                """;

            var anonymisedXml = await anonymiser.AnonymiseXmlAsync(xmlContent);
            Console.WriteLine("Original XML:");
            Console.WriteLine(xmlContent);
            Console.WriteLine("\nAnonymised XML:");
            Console.WriteLine(anonymisedXml);

            // Test CSV anonymization
            Console.WriteLine("\nTesting CSV anonymization...");
            var csvContent = """
                Name,Email,customerName,customerEmail,OtherField
                John Doe,john.doe@example.com,Jane Smith,jane.smith@example.com,Some other data
                Bob Johnson,bob.johnson@example.com,Alice Brown,alice.brown@example.com,Different data
                """;

            var anonymisedCsv = await anonymiser.AnonymiseCsvAsync(csvContent);
            Console.WriteLine("Original CSV:");
            Console.WriteLine(csvContent);
            Console.WriteLine("\nAnonymised CSV:");
            Console.WriteLine(anonymisedCsv);

            // Test consistency
            Console.WriteLine("\nTesting consistency...");
            var secondJsonContent = """
                {
                    "Name": "John Doe",
                    "Email": "john.doe@example.com",
                    "customerName": "Bob Johnson",
                    "customerEmail": "bob.johnson@example.com",
                    "OtherField": "Different data"
                }
                """;

            var secondAnonymisedJson = await anonymiser.AnonymiseJsonAsync(secondJsonContent);
            Console.WriteLine("Second JSON (same Name/Email):");
            Console.WriteLine(secondJsonContent);
            Console.WriteLine("\nAnonymised (should have same Name/Email values):");
            Console.WriteLine(secondAnonymisedJson);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
} 