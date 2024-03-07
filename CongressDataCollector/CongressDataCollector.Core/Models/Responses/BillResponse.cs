using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models.Responses;

public class BillResponse
{
    [JsonProperty("bill")]
    public Bill Bill { get; set; }
}