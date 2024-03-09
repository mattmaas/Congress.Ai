using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class OpenAiSummaries
{
    [JsonProperty("summary")]
    public string Summary { get; set; }
    [JsonProperty("keyChanges")]
    public List<string> KeyChanges { get; set; }
}