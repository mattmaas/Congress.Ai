using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class RelatedBill
{
    [JsonProperty("congress")]
    public int Congress { get; set; }

    [JsonProperty("latestAction")]
    public LatestAction LatestAction { get; set; }

    [JsonProperty("number")]
    public string Number { get; set; }

    [JsonProperty("relationshipDetails")]
    public List<RelationshipDetail> RelationshipDetails { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}