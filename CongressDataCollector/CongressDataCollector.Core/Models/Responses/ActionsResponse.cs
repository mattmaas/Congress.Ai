using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models.Responses;

public class ActionsResponse : PagedResponse
{
    [JsonProperty("actions")]
    public List<LegislativeAction> Actions { get; set; }
}