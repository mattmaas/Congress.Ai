using System;
using System.Threading.Tasks;
using CongressDataCollector.Core.Interfaces;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models; // Make sure to include this using directive for OpenAI API SDK

namespace CongressDataCollector.Services;

public class OpenAiService : IOpenAiService
{
    private readonly OpenAIAPI _openAIApi;

    public OpenAiService()
    {
        var apiKey = Environment.GetEnvironmentVariable("OpenAiKey") ??
                     throw new InvalidOperationException("Open AI API KEY not found");
        _openAIApi = new OpenAIAPI(apiKey);
    }

    public async Task<string> AnalyzeBillTextAsync(string billText)
    {
        var chatRequest = new ChatRequest
        {
            Model = Model.GPT4_Turbo,
            Temperature = 0.7,
            MaxTokens = 1024,
            ResponseFormat = ChatRequest.ResponseFormats.JsonObject,
            Messages = new ChatMessage[]
            {
                new(ChatMessageRole.System, ConstructPrompt()),
                new(ChatMessageRole.User, billText)
            }
        };

        var results = await _openAIApi.Chat.CreateChatCompletionAsync(chatRequest);
        return results.Choices.First().Message.TextContent;
    }
    private static string ConstructPrompt()
    {
        return @"
Analyze the provided bill text and generate a response in JSON format that includes a brief summary and the most crucial key changes. The summary should distill the bill's primary goals and effects, excluding specific references like bill numbers which are provided elsewhere. Please start the summary by referencing the title of the bill rather than just calling it the 'bill'. Throughout both sections of your response, remember to use specific language rather than vague. This for an app that needs to educate a lot of people. Each key change should be concisely described, integrating its broader implications in a single sentence. Where relevant, include 'Affected parties' with who is impacted.

{
  ""summary"": ""Summarize the bill's main objectives and potential impacts concisely. Avoid referencing specific bill numbers or procedural details, focusing instead on the essence and implications of the legislation."",
  ""keyChanges"": [
    ""List the most significant changes proposed by the bill. Ensure all descriptions are clear and contained within single strings for consistency. Where applicable, incorporate 'Affected parties: ' at the end of the string to clarify who will be impacted by these changes, if applicable. At least one key change should include affected parties. Make sure no key change is structured differently than a single string. Less bullet points is better so consider grouping points or shaving off less important ones.""
  ]
}";
    }
}