using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class Stub
{
    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonIgnore]
    public bool HasStubData => Count > 0;
}