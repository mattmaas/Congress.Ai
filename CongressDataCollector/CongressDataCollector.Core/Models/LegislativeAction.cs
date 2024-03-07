using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class LegislativeAction
{
    [JsonProperty("actionCode")]
    public string ActionCode { get; set; }

    [JsonProperty("actionDate")]
    public DateTime? ActionDate { get; set; }

    [JsonProperty("committees")]
    public List<Committee> Committees { get; set; }

    [JsonProperty("sourceSystem")]
    public SourceSystem SourceSystem { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }
}