using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class CboCostEstimate
{
    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("pubDate")]
    public DateTime? PubDate { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}