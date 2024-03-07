using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models.Responses;

public class RelatedBillsResponse : PagedResponse
{
    [JsonProperty("relatedBills")]
    public List<RelatedBill> RelatedBills { get; set; }
}