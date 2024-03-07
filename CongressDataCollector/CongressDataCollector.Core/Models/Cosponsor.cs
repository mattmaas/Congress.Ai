using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class Cosponsor
{
    [JsonProperty("bioguideId")]
    public string BioguideId { get; set; }

    [JsonProperty("district")]
    public int District { get; set; }

    [JsonProperty("firstName")]
    public string FirstName { get; set; }

    [JsonProperty("fullName")]
    public string FullName { get; set; }

    [JsonProperty("isOriginalCosponsor")]
    public bool IsOriginalCosponsor { get; set; }

    [JsonProperty("lastName")]
    public string LastName { get; set; }

    [JsonProperty("middleName")]
    public string MiddleName { get; set; }

    [JsonProperty("party")]
    public string Party { get; set; }

    [JsonProperty("sponsorshipDate")]
    public DateTime? SponsorshipDate { get; set; }

    [JsonProperty("state")]
    public string State { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}