using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class Format
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}