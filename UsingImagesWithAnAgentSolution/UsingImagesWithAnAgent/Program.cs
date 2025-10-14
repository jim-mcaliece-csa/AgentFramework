using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System;
using System.Text.Json;
Console.WriteLine("Using Images with An Agent POC");
var endpoint = LoadSettings();
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new DefaultAzureCredential())
        .GetChatClient("gpt-4o")
        .CreateAIAgent(
            name: "VisionAgent",
            instructions: "You are a helpful agent that can analyze images");
ChatMessage message = new(ChatRole.User, [
    new TextContent("What do you see in this image?"),
    new UriContent("https://pngimg.com/uploads/spongebob/spongebob_PNG38.png", "image/jpeg")
]);

Console.WriteLine(await agent.RunAsync(message));
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
