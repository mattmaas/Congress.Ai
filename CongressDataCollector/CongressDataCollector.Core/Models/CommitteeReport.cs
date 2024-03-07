using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class CommitteeReport
{
    [JsonProperty("citation")]
    public string Citation { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}