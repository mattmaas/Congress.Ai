using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class SourceSystem
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }
}