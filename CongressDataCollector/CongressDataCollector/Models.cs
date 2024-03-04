
using static CongressDataCollector.MainFunction;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Azure.Core;

namespace CongressDataCollector.Models
{
    public class LatestAction
    {
        [JsonProperty("actionDate")]
        public DateTime? ActionDate { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class RootObject
    {
        [JsonProperty("bills")]
        public Bill[] Bills { get; set; }

        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }
    }

    public class Bill
    {
        [JsonIgnore]
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

        [JsonProperty("textVersions")]
        public Stub TextVersions { get; set; }

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
        // This method updates the current Bill object with detailed information from another Bill object
        public void UpdateWithDetailedInfo(Bill fetchedBill)
        {
            // Update the Bill object with detailed information
            this.ConstitutionalAuthorityStatementText = this.ConstitutionalAuthorityStatementText ?? fetchedBill.ConstitutionalAuthorityStatementText;
            this.IntroducedDate = this.IntroducedDate ?? fetchedBill.IntroducedDate;

            // Update collections from the detailed and general fetch responses
            this.CboCostEstimates = this.CboCostEstimates ?? fetchedBill.CboCostEstimates;
            this.CommitteeReports = this.CommitteeReports ?? fetchedBill.CommitteeReports;

            // Update stubs if the detailed or general fetch responses contain more information
            this.Laws = this.Laws ?? fetchedBill.Laws;
            this.Sponsors = this.Sponsors ?? fetchedBill.Sponsors;
            this.Actions = this.Actions?.Count > 0 ? this.Actions : fetchedBill.Actions;
            this.Amendments = this.Amendments?.Count > 0 ? this.Amendments : fetchedBill.Amendments;
            this.Committees = this.Committees?.Count > 0 ? this.Committees : fetchedBill.Committees;
            this.Cosponsors = this.Cosponsors?.Count > 0 ? this.Cosponsors : fetchedBill.Cosponsors;
            this.RelatedBills = this.RelatedBills?.Count > 0 ? this.RelatedBills : fetchedBill.RelatedBills;
            this.Subjects = this.Subjects?.Count > 0 ? this.Subjects : fetchedBill.Subjects;
            this.Summaries = this.Summaries?.Count > 0 ? this.Summaries : fetchedBill.Summaries;
            this.TextVersions = this.TextVersions?.Count > 0 ? this.TextVersions : fetchedBill.TextVersions;
            this.Titles = this.Titles?.Count > 0 ? this.Titles : fetchedBill.Titles;
        }
    }

    public class Stub
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonIgnore]
        public bool HasStubData => Count > 0;
    }
    public class LegislativeAction
    {
        [JsonProperty("actionCode")]
        public string ActionCode { get; set; }

        [JsonProperty("actionDate")]
        public DateTime? ActionDate { get; set; }

        [JsonProperty("committees")]
        public List<Committee> Committees { get; set; }

        [JsonProperty("sourceSystem")]
        public SourceSystem SourceSystem { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class BillResponse
    {
        [JsonProperty("bill")]
        public Bill Bill { get; set; }
    }

    public class Committee
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("systemCode")]
        public string SystemCode { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class SourceSystem
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

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

    public class PolicyArea
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class RelatedBill
    {
        [JsonProperty("congress")]
        public int Congress { get; set; }

        [JsonProperty("latestAction")]
        public LatestAction LatestAction { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("relationshipDetails")]
        public List<RelationshipDetail> RelationshipDetails { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class RelationshipDetail
    {
        [JsonProperty("identifiedBy")]
        public string IdentifiedBy { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Summary
    {
        [JsonProperty("actionDate")]
        public DateTime? ActionDate { get; set; }

        [JsonProperty("actionDesc")]
        public string ActionDesc { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("updateDate")]
        public DateTime? UpdateDate { get; set; }

        [JsonProperty("versionCode")]
        public string VersionCode { get; set; }
    }

    public class LegislativeSubject
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class TextVersion
    {
        [JsonProperty("date")]
        public DateTime? Date { get; set; }

        [JsonProperty("formats")]
        public List<Format> Formats { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Format
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class CboCostEstimate
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("pubDate")]
        public DateTime? PubDate { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class CommitteeReport
    {
        [JsonProperty("citation")]
        public string Citation { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class Law
    {
        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }


    public class Pagination
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }
    }

    // Response classes with pagination support
    public class PagedResponse
    {
        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }
    }

    public class ActionsResponse : PagedResponse
    {
        [JsonProperty("actions")]
        public List<LegislativeAction> Actions { get; set; }
    }

    public class CosponsorsResponse : PagedResponse
    {
        [JsonProperty("cosponsors")]
        public List<Cosponsor> Cosponsors { get; set; }
    }

    public class RelatedBillsResponse : PagedResponse
    {
        [JsonProperty("relatedBills")]
        public List<RelatedBill> RelatedBills { get; set; }
    }

    public class SubjectsResponse : PagedResponse
    {
        [JsonProperty("subjects")]
        public SubjectsContainer Subjects { get; set; }
    }

    public class SubjectsContainer
    {
        [JsonProperty("legislativeSubjects")]
        public List<LegislativeSubject> LegislativeSubjects { get; set; }

        [JsonProperty("policyArea")]
        public PolicyArea PolicyArea { get; set; }
    }

    public class SummariesResponse : PagedResponse
    {
        [JsonProperty("summaries")]
        public List<Summary> Summaries { get; set; }
    }

    public class TextVersionsResponse : PagedResponse
    {
        [JsonProperty("textVersions")]
        public List<TextVersion> TextVersions { get; set; }
    }

    public class FetchState
    {
        [JsonProperty("lastRequestUri")]
        public string LastRequestUri { get; set; }

        [JsonProperty("nextRequestUri")]
        public string NextRequestUri { get; set; }

        [JsonProperty("lastRunTime")]
        public DateTime LastRunTime { get; set; }

        [JsonProperty("lastFetchedBillShellActionDate")]
        public DateTime? LastFetchedBillShellActionDate { get; set; }

        [JsonProperty("lastProcessedBillId")]
        public string LastProcessedBillId { get; set; }

        [JsonProperty("rateLimitHitCount")]
        public int RateLimitHitCount { get; set; }

        [JsonProperty("lastSuccessfulUri")]
        public string LastSuccessfulUri { get; set; }
    }

    public class BillsFetchResult
    {
        [JsonProperty("bills")]
        public List<Bill> Bills { get; set; }

        [JsonProperty("updatedState")]
        public FetchState UpdatedState { get; set; }
    }

    // Additional models or helper classes as needed...
}