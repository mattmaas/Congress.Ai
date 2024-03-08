using Newtonsoft.Json;

namespace CongressDataCollector.Core.Models;

public class Bill
{
    [JsonProperty("id")]
    public string Id => $"{Type}{Number}-{Congress}";

    [JsonIgnore]
    public string ChamberSessionKey => $"{OriginChamber}-{Congress}";

    [JsonProperty("congress")]
    public int Congress { get; set; }

    [JsonProperty("latestAction")]
    public LatestAction LatestAction { get; set; }

    [JsonProperty("number")]
    public string Number { get; set; }

    [JsonProperty("originChamber")]
    public string OriginChamber { get; set; }

    [JsonProperty("originChamberCode")]
    public string OriginChamberCode { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("updateDate")]
    public DateTime? UpdateDate { get; set; }

    [JsonProperty("updateDateIncludingText")]
    public DateTime? UpdateDateIncludingText { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("introducedDate")]
    public DateTime? IntroducedDate { get; set; }

    [JsonProperty("constitutionalAuthorityStatementText")]
    public string ConstitutionalAuthorityStatementText { get; set; }

    [JsonProperty("sponsors")]
    public List<Cosponsor> Sponsors { get; set; }

    [JsonProperty("laws")]
    public List<Law> Laws { get; set; }

    [JsonProperty("actions")]
    public Stub Actions { get; set; }

    [JsonProperty("amendments")]
    public Stub Amendments { get; set; }

    [JsonProperty("cboCostEstimates")]
    public List<CboCostEstimate> CboCostEstimates { get; set; }

    [JsonProperty("committeeReports")]
    public List<CommitteeReport> CommitteeReports { get; set; }

    [JsonProperty("committees")]
    public Stub Committees { get; set; }

    [JsonProperty("cosponsors")]
    public Stub Cosponsors { get; set; }

    [JsonProperty("policyArea")]
    public PolicyArea PolicyArea { get; set; }

    [JsonProperty("relatedBills")]
    public Stub RelatedBills { get; set; }

    [JsonProperty("subjects")]
    public Stub Subjects { get; set; }

    [JsonProperty("summaries")]
    public Stub Summaries { get; set; }
    [JsonProperty("openAiSummaries")]
    public OpenAiSummaries? OpenAiSummaries { get; set; }
    

    [JsonProperty("textVersions")]
    public Stub TextVersions { get; set; }
    [JsonProperty("plainText")]
    public string PlainText { get; set; }


    [JsonProperty("titles")]
    public Stub Titles { get; set; }

    [JsonProperty("detailedActions")]
    public List<LegislativeAction> DetailedActions { get; set; }

    [JsonProperty("detailedCosponsors")]
    public List<Cosponsor> DetailedCosponsors { get; set; }

    [JsonProperty("detailedRelatedBills")]
    public List<RelatedBill> DetailedRelatedBills { get; set; }

    [JsonProperty("detailedSubjects")]
    public List<LegislativeSubject> DetailedSubjects { get; set; }

    [JsonProperty("detailedSummaries")]
    public List<Summary> DetailedSummaries { get; set; }

    [JsonProperty("detailedTextVersions")]
    public List<TextVersion> DetailedTextVersions { get; set; }
    public bool HasDetailedInfo { get; set; }

    // This method updates the current Bill object with detailed information from another Bill object
    public void UpdateWithDetailedInfo(Bill fetchedBill)
    {
        // Update the Bill object with detailed information
        ConstitutionalAuthorityStatementText = ConstitutionalAuthorityStatementText ?? fetchedBill.ConstitutionalAuthorityStatementText;
        IntroducedDate = IntroducedDate ?? fetchedBill.IntroducedDate;

        // Update collections from the detailed and general fetch responses
        CboCostEstimates = CboCostEstimates ?? fetchedBill.CboCostEstimates;
        CommitteeReports = CommitteeReports ?? fetchedBill.CommitteeReports;

        // Update stubs if the detailed or general fetch responses contain more information
        Laws = Laws ?? fetchedBill.Laws;
        Sponsors = Sponsors ?? fetchedBill.Sponsors;
        Actions = Actions?.Count > 0 ? Actions : fetchedBill.Actions;
        Amendments = Amendments?.Count > 0 ? Amendments : fetchedBill.Amendments;
        Committees = Committees?.Count > 0 ? Committees : fetchedBill.Committees;
        Cosponsors = Cosponsors?.Count > 0 ? Cosponsors : fetchedBill.Cosponsors;
        RelatedBills = RelatedBills?.Count > 0 ? RelatedBills : fetchedBill.RelatedBills;
        Subjects = Subjects?.Count > 0 ? Subjects : fetchedBill.Subjects;
        Summaries = Summaries?.Count > 0 ? Summaries : fetchedBill.Summaries;
        TextVersions = TextVersions?.Count > 0 ? TextVersions : fetchedBill.TextVersions;
        Titles = Titles?.Count > 0 ? Titles : fetchedBill.Titles;
    }
}