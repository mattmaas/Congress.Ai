using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class Summary
{
    [JsonProperty("actionDate")]
    public DateTime? ActionDate { get; set; }

    [JsonProperty("actionDesc")]
    public string ActionDesc { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("updateDate")]
    public DateTime? UpdateDate { get; set; }

    [JsonProperty("versionCode")]
    public string VersionCode { get; set; }
}