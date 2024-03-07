using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class Committee
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("systemCode")]
    public string SystemCode { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}