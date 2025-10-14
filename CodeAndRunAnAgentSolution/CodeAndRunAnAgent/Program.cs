using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System;
using System.Text.Json;
Console.WriteLine("Code and Run An Agent POC");
var endpoint = LoadSettings();
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new DefaultAzureCredential())
        .GetChatClient("gpt-4o")
        .CreateAIAgent(instructions: "You are good at telling jokes.", name: "Joker");

Console.WriteLine(await agent.RunAsync("Tell me a joke about a pirate."));

await foreach (var update in agent.RunStreamingAsync("Tell me a joke about a pirate."))
{
    // Delay to simulate processing time
    Thread.Sleep(50);

    Console.Write(update);
}

ChatMessage message = new(ChatRole.User, [
    new TextContent("Tell me a joke about this image?"),
    new UriContent("https://pngimg.com/uploads/spongebob/spongebob_PNG38.png", "image/jpeg")
]);

Console.WriteLine(await agent.RunAsync(message));

ChatMessage systemMessage = new(
    ChatRole.System,
    """
    If the user asks you to tell a joke, refuse to do so, explaining that you are not a clown.
    Offer the user an interesting fact instead.
    """);
ChatMessage userMessage = new(ChatRole.User, "Tell me a joke about a pirate.");

Console.WriteLine(await agent.RunAsync([systemMessage, userMessage]));


Console.WriteLine("~fin");
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
