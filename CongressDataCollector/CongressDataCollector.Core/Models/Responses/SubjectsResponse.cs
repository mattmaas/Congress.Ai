using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models.Responses;

public class SubjectsResponse : PagedResponse
{
    [JsonProperty("subjects")]
    public SubjectsContainer Subjects { get; set; }
}