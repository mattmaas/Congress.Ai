using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models.Responses;

public class RootObject
{
    [JsonProperty("bills")]
    public Bill[] Bills { get; set; }

    [JsonProperty("pagination")]
    public Pagination Pagination { get; set; }
}