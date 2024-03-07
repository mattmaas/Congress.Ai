using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models.Responses;

public class SummariesResponse : PagedResponse
{
    [JsonProperty("summaries")]
    public List<Summary> Summaries { get; set; }
}