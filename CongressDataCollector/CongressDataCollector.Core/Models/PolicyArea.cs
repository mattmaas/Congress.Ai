using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class PolicyArea
{
    [JsonProperty("name")]
    public string Name { get; set; }
}