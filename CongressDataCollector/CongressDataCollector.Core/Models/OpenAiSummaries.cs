using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class OpenAiSummaries
{
    [JsonProperty("summary")]
    public string Summary { get; set; }
    [JsonProperty("keyPoints")]
    public List<string> KeyPoints { get; set; }
}