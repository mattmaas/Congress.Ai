using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class LegislativeSubject
{
    [JsonProperty("name")]
    public string Name { get; set; }
}