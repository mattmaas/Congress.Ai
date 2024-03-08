using System.Text;
using CongressDataCollector.Core.Interfaces;
using Newtonsoft.Json;

namespace CongressDataCollector.Services;

public class OpenAiService : IOpenAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _oaiApiKey; 

    public OpenAiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_oaiApiKey}");

        _oaiApiKey = Environment.GetEnvironmentVariable("OpenAiKey") ?? throw new InvalidOperationException("open ai API KEY not found");
    }

    public async Task<string> AnalyzeBillTextAsync(string billText)
    {
        var prompt = ConstructPrompt(billText); // Implement this based on your specific prompt requirements
        var payload = new
        {
            model = "gpt-4-turbo-preview",
            prompt,
            temperature = 0.7,
            max_tokens = 1024
        };

        var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("https://api.openai.com/v1/completions", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);

        return responseObject.choices[0].text;
    }

    private static string ConstructPrompt(string billText)
    {
        return $@"
Answer the following questions in JSON format with the specified structure, based on the analysis of the provided text:

{{
  ""summary"": ""Provide a summary of the text here."",
  ""keyPoints"": [
    ""List key points in bullet point form here, including who will likely be affected and how.""
  ]
}}

Analyze the following bill:

""{billText}""
";
    }
}