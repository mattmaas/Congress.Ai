using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models.Responses;

public class CosponsorsResponse : PagedResponse
{
    [JsonProperty("cosponsors")]
    public List<Cosponsor> Cosponsors { get; set; }
}