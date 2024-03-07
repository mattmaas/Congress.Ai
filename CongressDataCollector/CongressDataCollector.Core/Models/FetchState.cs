using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class FetchState
{
    [JsonProperty("latestFetchedBillActionDate")]
    public DateTime? LatestFetchedBillActionDate { get; set; }
    [JsonProperty("earliestFetchedBillActionDate")]
    public DateTime? EarliestFetchedBillActionDate { get; set; }
    [JsonProperty("lastSuccessfulUri")]
    public string LastSuccessfulUri { get; set; }
}