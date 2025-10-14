using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System;
using System.Text.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
Console.WriteLine("Using Images with An Agent POC");
var endpoint = LoadSettings();
ChatOptions chatOptions = new()
{
    ResponseFormat = ChatResponseFormatJson.ForJsonSchema(
        schema: AIJsonUtilities.CreateJsonSchema(typeof(PersonInfo)),
        schemaName: "PersonInfo",
        schemaDescription: "Information about a person including their name, age, and occupation")
};

AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new DefaultAzureCredential())
        .GetChatClient("gpt-4o")
        .CreateAIAgent(new ChatClientAgentOptions()
        {
            Name = "HelpfulAssistant",
            Instructions = "You are a helpful assistant.",
            ChatOptions = chatOptions
        });
var response = await agent.RunAsync("Please provide information about John Smith, who is a 35-year-old software engineer.");
var personInfo = response.Deserialize<PersonInfo>(JsonSerializerOptions.Web);
Console.WriteLine($"Name: {personInfo.Name}, Age: {personInfo.Age}, Occupation: {personInfo.Occupation}");
var updates = agent.RunStreamingAsync("Please provide information about John Smith, who is a 35-year-old software engineer.");
personInfo = (await updates.ToAgentRunResponseAsync()).Deserialize<PersonInfo>(JsonSerializerOptions.Web);


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

public class PersonInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("age")]
    public int? Age { get; set; }

    [JsonPropertyName("occupation")]
    public string? Occupation { get; set; }
}



