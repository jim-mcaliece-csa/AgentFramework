// See https://aka.ms/new-console-template for more information
using System.Text.Json;
using System;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI;

Console.WriteLine("Persisting and Resuming Agent Conversations");

AIAgent agent = new AzureOpenAIClient(
    new Uri(LoadSettings()),
    new AzureCliCredential())
     .GetChatClient("gpt-4o")
     .CreateAIAgent(instructions: "You are a helpful assistant.", name: "Assistant");

AgentThread thread = agent.GetNewThread();

// Run the agent and append the exchange to the thread
Console.WriteLine(await agent.RunAsync("Tell me a short pirate joke.", thread));

JsonElement serializedThread = thread.Serialize();
string serializedJson = JsonSerializer.Serialize(serializedThread, JsonSerializerOptions.Web);

// Example: save to a local file (replace with DB or blob storage in production)
string filePath = Path.Combine(Path.GetTempPath(), "agent_thread.json");
await File.WriteAllTextAsync(filePath, serializedJson);

// Read persisted JSON
string loadedJson = await File.ReadAllTextAsync(filePath);
JsonElement reloaded = JsonSerializer.Deserialize<JsonElement>(loadedJson);

// Deserialize the thread into an AgentThread tied to the same agent type
AgentThread resumedThread = agent.DeserializeThread(reloaded);

// Continue the conversation with resumed thread
Console.WriteLine(await agent.RunAsync("Now tell that joke in the voice of a pirate.", resumedThread));


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
