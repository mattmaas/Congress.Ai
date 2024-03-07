using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class RelationshipDetail
{
    [JsonProperty("identifiedBy")]
    public string IdentifiedBy { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }
}