using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class Pagination
{
    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonProperty("next")]
    public string Next { get; set; }
}