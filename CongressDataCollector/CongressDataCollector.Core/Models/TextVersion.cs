using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class TextVersion
{
    [JsonProperty("date")]
    public DateTime? Date { get; set; }

    [JsonProperty("formats")]
    public List<Format> Formats { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }
}