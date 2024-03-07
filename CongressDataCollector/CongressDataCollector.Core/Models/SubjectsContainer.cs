using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class SubjectsContainer
{
    [JsonProperty("legislativeSubjects")]
    public List<LegislativeSubject> LegislativeSubjects { get; set; }

    [JsonProperty("policyArea")]
    public PolicyArea PolicyArea { get; set; }
}