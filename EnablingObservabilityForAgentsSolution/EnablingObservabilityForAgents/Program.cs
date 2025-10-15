// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System.Text.Json;

Console.WriteLine("Agent Framework Observability");
// Create a TracerProvider that exports to the console
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource("agent-telemetry-source")
    .AddConsoleExporter()
    .Build();


AIAgent agent = new AzureOpenAIClient(
    new Uri(LoadSettings()),
    new DefaultAzureCredential())
        .GetChatClient("gpt-4o")
        .CreateAIAgent(instructions: "You are good at telling jokes.", name: "Joker")
        .AsBuilder()
        .UseOpenTelemetry(sourceName: "agent-telemetry-source")
        .Build();
Console.WriteLine(await agent.RunAsync("Tell me a joke about a pirate."));
static string LoadSettings()
{
    string filePath = "local.settings.json";

    if (!File.Exists(filePath))
    {
        Console.WriteLine("local.settings.json not found.");
        return string.Empty;
    }

    string jsonContent = File.ReadAllText(filePath);

    using JsonDocument doc = JsonDocument.Parse(jsonContent);
    JsonElement root = doc.RootElement;

    if (root.TryGetProperty("Values", out JsonElement values))
    {
        Console.WriteLine("Loading Settings");
        foreach (JsonProperty setting in values.EnumerateObject())
        {
            // Add each setting to the dictionary
            if (setting.Name == "Endpoint")
            {
                return setting.Value.GetString() ?? string.Empty;
            }
        }
    }
    else
    {
        Console.WriteLine("No 'Values' section found in local.settings.json.");
    }

    return string.Empty;
}
