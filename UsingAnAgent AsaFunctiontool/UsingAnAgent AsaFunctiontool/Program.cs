using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
Console.WriteLine("Using Images with An Agent POC");
var endpoint = LoadSettings();

AIAgent weatherAgent = new AzureOpenAIClient(
    new Uri(endpoint),
    new DefaultAzureCredential())
     .GetChatClient("gpt-4o")
     .CreateAIAgent(
        instructions: "You answer questions about the weather.",
        name: "WeatherAgent",
        description: "An agent that answers questions about the weather.",
        tools: [AIFunctionFactory.Create(GetWeather)]);

AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new DefaultAzureCredential())
     .GetChatClient("gpt-4o")
     .CreateAIAgent(instructions: "You are a helpful assistant who responds in French.", tools: [weatherAgent.AsAIFunction()]);

Console.WriteLine(await agent.RunAsync("What is the weather like in Marlton NJ?"));
[Description("Get the weather for a given location.")]
static string GetWeather([Description("The location to get the weather for.")] string location)
    => $"The weather in {location} is cloudy with a high of 15°C.";
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
