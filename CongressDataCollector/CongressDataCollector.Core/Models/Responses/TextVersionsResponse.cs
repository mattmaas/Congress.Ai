using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models.Responses;

public class TextVersionsResponse : PagedResponse
{
    [JsonProperty("textVersions")]
    public List<TextVersion> TextVersions { get; set; }
}