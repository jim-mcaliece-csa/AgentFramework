using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System;
using System.Text.Json;
Console.WriteLine("Multi-turnConversationsWithAnAgent");
var endpoint = LoadSettings();
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new DefaultAzureCredential())
        .GetChatClient("gpt-4o")
        .CreateAIAgent(instructions: "You are good at telling jokes.", name: "Joker");
AgentThread thread = agent.GetNewThread();

Console.WriteLine(await agent.RunAsync("Tell me a joke about a pirate.", thread));
Console.WriteLine(await agent.RunAsync("How many words in the punchline to the joke?", thread));

AgentThread thread1 = agent.GetNewThread();
AgentThread thread2 = agent.GetNewThread();
Console.WriteLine(await agent.RunAsync("Tell me a joke about a pirate.", thread1));
Console.WriteLine(await agent.RunAsync("Tell me a joke about a robot.", thread2));
Console.WriteLine(await agent.RunAsync("How many words in the punchline to the joke?", thread1));
Console.WriteLine(await agent.RunAsync("How many words in the punchline to the joke?", thread2));
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