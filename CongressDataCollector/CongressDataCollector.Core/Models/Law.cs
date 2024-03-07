using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class Law
{
    [JsonProperty("number")]
    public string Number { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }
}