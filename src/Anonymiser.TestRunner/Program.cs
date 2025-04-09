using Anonymiser;
using Anonymiser.Models.Configuration;

namespace Anonymiser.TestRunner;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Anonymiser Test Runner");
            Console.WriteLine("=====================");

            // Initialize anonymiser with config
            var anonymiser = new Anonymiser("config.json");

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