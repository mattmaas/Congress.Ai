using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models.Responses;

public class PagedResponse
{
    [JsonProperty("pagination")]
    public Pagination Pagination { get; set; }
}