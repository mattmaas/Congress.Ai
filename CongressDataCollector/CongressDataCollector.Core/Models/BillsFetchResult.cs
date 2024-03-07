using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class BillsFetchResult
{
    [JsonProperty("bills")]
    public List<Bill> Bills { get; set; }

    [JsonProperty("updatedState")]
    public FetchState UpdatedState { get; set; }
}