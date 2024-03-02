using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static CongressDataCollector.MainFunction;

namespace CongressDataCollector
{
    public class MainFunction
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private readonly Microsoft.Azure.Cosmos.Container _container;

        public MainFunction()
        {
            var cosmosClient = new CosmosClient(Environment.GetEnvironmentVariable("CosmosDBConnection"));
            _container = cosmosClient.GetContainer("PolitiSightDB", "Bills");
        }

        [FunctionName("MainFunction")]
        public void Run([TimerTrigger("0 3 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            return;
            RunAsync(log); // This blocks the thread, not ideal but necessary for timer triggers
        }
        public async Task RunAsync(ILogger log)
        {
            var state = new FetchState();
            var billFetchResult = await FetchBills(state, log);
            state.LastFetchedBillShellActionDate = billFetchResult.Bills[0]?.latestAction?.actionDate;//.FirstOrDefault()?.latestAction.actionDate;

            foreach (var bill in billFetchResult.Bills)
            {
                //// Skip already processed bills based on LastProcessedBillId
                //if (!string.IsNullOrEmpty(state.) && bill.number.CompareTo(state.LastProcessedBillId) <= 0)
                //{
                //    continue;
                //}
                FetchBillDetails((bill, state), log);

                // Update and save state after each successful bill detail fetch
                state.LastProcessedBillId = bill.id;
                //SaveFetchState(state, log);
            }
            log.LogInformation("done!");
        }
        public async Task<BillsFetchResult> FetchBills(FetchState state, ILogger log)
    {
        log.LogInformation($"FetchBills function executed at: {DateTime.Now}");
        var fetchedBills = new List<Bill>();
        var apiKey = Environment.GetEnvironmentVariable("CongressAPIKEY");
        string baseUri = "https://api.congress.gov/v3/bill/118?sort=updateDate+desc&limit=250";
        string requestUri = state.NextRequestUri ?? $"{baseUri}&api_key={apiKey}";

        while (!string.IsNullOrEmpty(requestUri) )//&& fetchedBills.Count<750)
        {
            var (response, updatedState) =  await SendRequestWithRetry(HttpClient, requestUri, state, log);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<RootObject>(content);
                    fetchedBills.AddRange(responseData?.bills);
                    log.LogInformation($"Fetched {responseData.bills.Length} bills. Response status: {response.StatusCode}. Progress: {fetchedBills.Count}/{responseData.pagination.count}");

                    // Update the state with the last successful URI and potentially other state updates from SendRequestWithRetry
                    state = updatedState;
                    requestUri = $"{responseData.pagination.next}&api_key={apiKey}"; // Prepare the next request URI for the loop

                    if (responseData.bills.Length < 250)
                    {
                        break; // Exit the loop if no bills are returned in the response
                    }
            }
            else
            {
                log.LogError($"Failed to fetch data. Status: {response.StatusCode}");
                // Consider breaking or handling the error as appropriate for your application
                break;
            }
        }

        // Update the state's NextRequestUri to continue from the last successful position in the next execution
        state.NextRequestUri = requestUri;

        return new BillsFetchResult
        {
            Bills = fetchedBills.Count > 0 ? fetchedBills : new List<Bill>(),
            UpdatedState = state
        };
        }

        public static async Task<FetchState> FetchBillDetails((Bill bill, FetchState state) input, ILogger log)
        {
            var (bill, state) = input;

            // Pass state to each detailed fetch function and get the updated state back
            state = await FetchGeneralBillDetails(bill, state, log);
            if(bill.actions?.count > 0)
            {
                state = await FetchDetailedActions(bill, state, log);
            }
            if (bill.cosponsors?.count > 0)
            {
                state = await FetchDetailedCosponsors(bill, state, log);
            }
            if (bill.relatedBills?.count > 0)
            {
                state = await FetchDetailedRelatedBills(bill, state, log);
            }
            if (bill.subjects?.count > 0)
            {
                state = await FetchDetailedSubjects(bill, state, log);
            }
            if (bill.summaries?.count > 0)
            {
                state = await FetchDetailedSummaries(bill, state, log);
            }
            if (bill.textVersions?.count > 0)
            {
                state = await FetchDetailedTextVersions(bill, state, log);
            }

            log.LogInformation($"Aggregated detailed information for bill {bill?.number}:");
            log.LogInformation(bill?.ToString());

            return state;
        }

        private static async Task<FetchState> FetchDetailedRelatedBills(Bill bill, FetchState state, ILogger log)
        {
            if (bill.number == "2670")
            {
                log.LogInformation("About to break!!!");
            }
            var apiKey = Environment.GetEnvironmentVariable("CongressAPIKEY");
            var apiUrl = $"{bill.relatedBills?.url}&limit=250&api_key={apiKey}";
            bill.detailedRelatedBills = Array.Empty<RelatedBill>();
            FetchState updatedState = state;

            while (!string.IsNullOrEmpty(apiUrl))
            {
                var (response, tempState) = await SendRequestWithRetry(HttpClient, apiUrl, updatedState, log);
                updatedState = tempState; // Update the state with the potentially new state from the retry logic

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var relatedBillsResponse = JsonConvert.DeserializeObject<RelatedBillsResponse>(content);

                    var newRelatedBills = relatedBillsResponse.relatedBills.Select(rb => new RelatedBill
                    {
                        congress = rb.congress,
                        latestAction = new LatestAction
                        {
                            actionDate = DateTime.Parse(rb.latestAction.actionDate),
                            text = rb.latestAction.text
                        },
                        number = rb.number.ToString(),
                        relationshipDetails = rb.relationshipDetails.Select(rd => new RelationshipDetail
                        {
                            identifiedBy = rd.identifiedBy,
                            type = rd.type
                        }).ToArray(),
                        title = rb.title,
                        type = rb.type,
                        url = rb.url
                    }).ToList();

                    bill.detailedRelatedBills = bill.detailedRelatedBills.Concat(newRelatedBills).ToArray();

                    apiUrl = relatedBillsResponse.pagination.next; // Update apiUrl for next iteration
                    log.LogInformation($"Fetched page of related bills for {bill.number}");
                    if (relatedBillsResponse.relatedBills.Length < 250)
                    {
                        break; 
                    }
                }
                else
                {
                    log.LogError($"Failed to fetch related bills for {bill.number}. Status: {response.StatusCode}");
                    break; // Exit loop on failure
                }
                }

            return updatedState;
        }

        private static async Task<FetchState> FetchDetailedSubjects(Bill bill, FetchState state, ILogger log)
        {
            if (bill.number == "2670")
            {
                log.LogInformation("About to break!!!");
            }
            var apiKey = Environment.GetEnvironmentVariable("CongressAPIKEY");
            var apiUrl = $"{bill.subjects?.url}&limit=250&api_key={apiKey}";
            bill.detailedSubjects = Array.Empty<LegislativeSubject>();
            FetchState updatedState = state;

            while (!string.IsNullOrEmpty(apiUrl))
            {
                var (response, tempState) = await SendRequestWithRetry(HttpClient, apiUrl, updatedState, log);
                updatedState = tempState; // Update the state with potentially new state from the retry logic

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var subjectsResponse = JsonConvert.DeserializeObject<SubjectsResponse>(content);

                    bill.detailedSubjects = bill.detailedSubjects.Concat(subjectsResponse.subjects.legislativeSubjects.Select(ls => new LegislativeSubject
                    {
                        name = ls.name
                    })).ToArray();

                    bill.policyArea = new Policyarea
                    {
                        name = subjectsResponse.subjects.policyArea?.name
                    };

                    apiUrl = subjectsResponse.pagination.next; // Update apiUrl for the next page
                    log.LogInformation($"Fetched page of subjects for {bill.number}");
                    if (subjectsResponse.subjects.legislativeSubjects.Length < 250)
                    {
                        break;
                    }
                }
                else
                {
                    log.LogError($"Failed to fetch subjects for {bill.number}. Status: {response.StatusCode}");
                    break; // Exit loop on failure
                }
            }

            return updatedState;
        }
        private static async Task<FetchState> FetchDetailedSummaries(Bill bill, FetchState state, ILogger log)
        {
            var apiKey = Environment.GetEnvironmentVariable("CongressAPIKEY");
            var apiUrl = $"{bill.summaries?.url}&limit=250&api_key={apiKey}";
            bill.detailedSummaries = Array.Empty<Summary>();
            FetchState updatedState = state;

            while (!string.IsNullOrEmpty(apiUrl))
            {
                var (response, tempState) = await SendRequestWithRetry(HttpClient, apiUrl, updatedState, log);
                updatedState = tempState; // Update the state with potentially new state from the retry logic

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var summariesResponse = JsonConvert.DeserializeObject<SummariesResponse>(content);
                    bill.detailedSummaries = bill.detailedSummaries.Concat(summariesResponse.summaries.Select(s => new Summary
                    {
                        actionDate = DateTime.Parse(s.actionDate),
                        actionDesc = s.actionDesc,
                        text = s.text,
                        updateDate = DateTime.Parse(s.updateDate),
                        versionCode = s.versionCode
                    })).ToArray();

                    apiUrl = summariesResponse.pagination.next; // Update apiUrl for the next page
                    log.LogInformation($"Fetched page of summaries for {bill.number}");
                    if (summariesResponse.summaries?.Length < 250)
                    {
                        break;
                    }
                }
                else
                {
                    log.LogError($"Failed to fetch summaries for {bill.number}. Status: {response.StatusCode}");
                    break; // Exit loop on failure
                }
            }

            return updatedState;
        }
        private static async Task<FetchState> FetchGeneralBillDetails(Bill bill, FetchState state, ILogger log)
        {
            var apiKey = Environment.GetEnvironmentVariable("CongressAPIKEY");
            var apiUrl = $"{bill?.url}&limit=250&api_key={apiKey}";
            var (response, updatedState) = await SendRequestWithRetry(HttpClient, apiUrl, state, log);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var billResponse = JsonConvert.DeserializeObject<BillResponse>(content);

                if (billResponse != null && billResponse.bill != null)
                {
                    var detailedBill = billResponse.bill;
                    // Update the Bill object with detailed information
                    bill.constitutionalAuthorityStatementText = detailedBill.constitutionalAuthorityStatementText ?? bill.constitutionalAuthorityStatementText;
                    bill.introducedDate = detailedBill.introducedDate ?? bill.introducedDate;

                    // Update collections from the detailed response
                    bill.cboCostEstimates = detailedBill.cboCostEstimates ?? bill.cboCostEstimates;
                    bill.committeeReports = detailedBill.committeeReports ?? bill.committeeReports;
                    bill.laws = detailedBill.laws ?? bill.laws;
                    bill.sponsors = detailedBill.sponsors ?? bill.sponsors;

                    // Update stubs if the detailed response contains more information
                    bill.actions = detailedBill.actions?.count > 0 ? detailedBill.actions : bill.actions;
                    bill.amendments = detailedBill.amendments?.count > 0 ? detailedBill.amendments : bill.amendments;
                    bill.committees = detailedBill.committees?.count > 0 ? detailedBill.committees : bill.committees;
                    bill.cosponsors = detailedBill.cosponsors?.count > 0 ? detailedBill.cosponsors : bill.cosponsors;
                    bill.relatedBills = detailedBill.relatedBills?.count > 0 ? detailedBill.relatedBills : bill.relatedBills;
                    bill.subjects = detailedBill.subjects?.count > 0 ? detailedBill.subjects : bill.subjects;
                    bill.summaries = detailedBill.summaries?.count > 0 ? detailedBill.summaries : bill.summaries;
                    bill.textVersions = detailedBill.textVersions?.count > 0 ? detailedBill.textVersions : bill.textVersions;
                    bill.titles = detailedBill.titles?.count > 0 ? detailedBill.titles : bill.titles;

                    log.LogInformation($"Fetched and updated general bill details for {bill.number}");
                }
                else
                {
                    log.LogError($"Failed to fetch general bill details for {bill.number}. Status: {response.StatusCode}");
                }

            }
            return updatedState;
        }
        private static async Task<FetchState> FetchDetailedActions(Bill bill, FetchState state, ILogger log)
        {
            if (bill.number == "2670")
            {
                log.LogInformation("About to break!!!");
            }
            var apiKey = Environment.GetEnvironmentVariable("CongressAPIKEY");
            var apiUrl = $"{bill.actions?.url}&limit=250&api_key={apiKey}";
            bill.detailedActions = Array.Empty<Action>();
            FetchState updatedState = state;

            while (!string.IsNullOrEmpty(apiUrl))
            {
                var (response, tempState) = await SendRequestWithRetry(HttpClient, apiUrl, updatedState, log);
                updatedState = tempState;

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var actionsResponse = JsonConvert.DeserializeObject<ActionsResponse>(content);

                    var newActions = actionsResponse.actions.Select(a => new Action
                    {
                        actionCode = a.actionCode,
                        actionDate = DateTime.Parse(a.actionDate),
                        committees = a.committees?.Select(c => new Committee
                        {
                            name = c.name,
                            systemCode = c.systemCode,
                            url = c.url
                        }).ToArray(),
                        sourceSystem = new SourceSystem
                        {
                            code = a.sourceSystem != null ? a.sourceSystem.code : 0,
                            name = a.sourceSystem?.name,
                        },
                        text = a.text,
                        type = a.type
                    }).ToArray();

                    bill.detailedActions = bill.detailedActions.Concat(newActions).ToArray();

                    // Check for next page
                    apiUrl = actionsResponse.pagination.next;

                    log.LogInformation($"Fetched page of detailed actions for {bill.number}");
                    if (actionsResponse.actions.Length < 250)
                    {
                        break;
                    }
                }
                else
                {
                    log.LogError($"Failed to fetch detailed actions for {bill.number}. Status: {response.StatusCode}");
                    break; // Exit loop on failure
                }
            }

            return updatedState;
        }
        private static async Task<FetchState> FetchDetailedCosponsors(Bill bill, FetchState state, ILogger log)
        {
            if(bill.number == "1668" || bill.number == "4010" )
            {
                log.LogInformation("About to break!!!");
            }
            var apiKey = Environment.GetEnvironmentVariable("CongressAPIKEY");
            var apiUrl = $"{bill.cosponsors?.url}&limit=250&api_key={apiKey}";
            bill.detailedCosponsors = Array.Empty<Cosponsor>();
            FetchState updatedState = state;

            while (!string.IsNullOrEmpty(apiUrl))
            {
                var (response, tempState) = await SendRequestWithRetry(HttpClient, apiUrl, updatedState, log);
                updatedState = tempState; // Update the state with the potentially new state from the retry logic

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var cosponsorsResponse = JsonConvert.DeserializeObject<CosponsorsResponse>(content);

                    var newCosponsors = cosponsorsResponse.cosponsors.Select(c => new Cosponsor
                    {
                        bioguideId = c.bioguideId,
                        district = c.district,
                        firstName = c.firstName,
                        fullName = c.fullName,
                        isOriginalCosponsor = c.isOriginalCosponsor,
                        lastName = c.lastName,
                        middleName = c.middleName,
                        party = c.party,
                        sponsorshipDate = DateTime.Parse(c.sponsorshipDate),
                        state = c.state,
                        url = c.url
                    }).ToList();

                    bill.detailedCosponsors = bill.detailedCosponsors.Concat(newCosponsors).ToArray();

                    apiUrl = cosponsorsResponse.pagination.next; // Update apiUrl for next iteration
                    log.LogInformation($"Fetched page of detailed cosponsors for {bill.number}");
                    if (cosponsorsResponse.cosponsors.Length < 250)
                    {
                        break;
                    }
                }
                else
                {
                    log.LogError($"Failed to fetch detailed cosponsors for {bill.number}. Status: {response.StatusCode}");
                    break; // Exit loop on failure
                }
            }

            return updatedState;
        }
        public static async Task<(HttpResponseMessage, FetchState)> SendRequestWithRetry(HttpClient client, string requestUri, FetchState state, ILogger log, int maxRetries = 3)
        {
            HttpResponseMessage response = null;
            int retryCount = 0;
            do
            {
                response = await client.GetAsync(requestUri);

                if (!response.IsSuccessStatusCode && (int)response.StatusCode == 429) // HTTP 429 is the Too Many Requests status code
                {
                    retryCount++;
                    int waitTime = response.Headers.RetryAfter.Delta.HasValue ? (int)response.Headers.RetryAfter.Delta.Value.TotalSeconds : 60; // Default to 60 seconds if the header is not present
                    log.LogWarning($"Rate limit hit, waiting for {waitTime} seconds. Retry attempt {retryCount}.");
                    await Task.Delay(waitTime * 1000); // Wait for the specified time before retrying

                    // Update state here if needed, e.g., increment a counter for rate limit hits
                    state.RateLimitHitCount++;
                }
                else
                {
                    // Update the state with the last successful URI
                    state.LastSuccessfulUri = requestUri;
                    break; // Exit the loop if the response is successful or the error is not due to rate limiting
                }
            }
            while (retryCount < maxRetries);

            return (response, state); // Return the last response and the updated state
        }

        private static async Task<FetchState> FetchDetailedTextVersions(Bill bill, FetchState state, ILogger log)
        {
            // Check if bill or its textVersions or apiUrl is null before proceeding
            if (bill?.textVersions?.url == null)
            {
                log.LogWarning($"Bill {bill?.number} has no text versions or URL.");
                return state; // Return early if there's nothing to process
            }

            var apiKey = Environment.GetEnvironmentVariable("CongressAPIKEY");
            var apiUrl = $"{bill?.textVersions?.url}&limit=250&api_key={apiKey}";
            var updatedState = state;
            bill.detailedTextVersions = Array.Empty<TextVersion>();
            while (!string.IsNullOrEmpty(apiUrl))
            {
                var (response, tempState) = await SendRequestWithRetry(HttpClient, apiUrl, updatedState, log);
                updatedState = tempState; // Update the state with potentially new state from the retry logic

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var textVersionsResponse = JsonConvert.DeserializeObject<TextVersionsResponse>(content);

                    if (textVersionsResponse?.textVersions != null) // Check if textVersions is not null
                    {
                        foreach (var tv in textVersionsResponse.textVersions)
                        {
                            if (tv == null) continue; // Skip if the current text version is null

                            DateTime? parsedDate = null;
                            if (DateTime.TryParse(tv.date, out var tempDate))
                            {
                                parsedDate = tempDate;
                            }

                            var newVersion = new TextVersion
                            {
                                date = parsedDate,
                                formats = tv?.formats?.Select(f => new Format // Use null-conditional for formats
                                {
                                    type = f?.type, // Use null-conditional for each property
                                    url = f?.url
                                }).ToArray(),
                                type = tv?.type
                            };

                            bill.detailedTextVersions = bill.detailedTextVersions.Append(newVersion).ToArray(); // Safely append newVersion
                        }
                    }

                    apiUrl = textVersionsResponse?.pagination?.next; // Use null-conditional for pagination and next
                    log.LogInformation($"Fetched page of text versions for {bill.number}");
                }
                else
                {
                    log.LogError($"Failed to fetch text versions for {bill.number}. Status: {response.StatusCode}");
                    break; // Exit loop on failure
                }
            }

            return updatedState;
        }

        public class FetchState
    {
        public string LastRequestUri { get; set; }
        public string NextRequestUri { get; set; }
        public DateTime LastRunTime { get; set; }
        public DateTime? LastFetchedBillShellActionDate { get; set; }
        // Add more properties as needed to track state
        public string LastProcessedBillId { get; set; }
        public int RateLimitHitCount { get; internal set; }
        public string LastSuccessfulUri { get; internal set; }
    }
    public class LegislationData
    {
        public LegislationItem[] results { get; set; }
    }
        public class BillResponse
        {
            public Bill bill { get; set; }
            public Request request { get; set; }
        }

        public class LegislationItem
    {
        public string id { get; set; }
        // Other properties...
    }

    public class RootObject
    {
        public Bill[] bills { get; set; }
        public Bill bill { get; set; }
        public Pagination pagination { get; set; }
        public Request request { get; set; }

        public RelatedBill[] relatedBills { get; set; }
        public Subjects subjects { get; set; }
        public Summary[] summaries { get; set; }
        public TextVersion[] textVersions { get; set; }
    }

    public class Request
    {
        public string congress { get; set; }
        public string contentType { get; set; }
        public string format { get; set; }

        public string billNumber { get; set; }
        public string billType { get; set; }
        public string billUrl { get; set; }
    }


    public class LatestAction
    {
        public DateTime? actionDate { get; set; }
        public string text { get; set; }
        public string actionTime { get; set; }
    }
    public class Bill
    {
        public string id => $"{type}{number}-{congress}";

        // New property for the combined partition key
        [JsonIgnore] // Optionally ignore this property during serialization if it's only used for partitioning
        public string ChamberSessionKey => $"{originChamber}-{congress}";
        public int congress { get; set; }
        public LatestAction latestAction { get; set; }
        public string number { get; set; }
        public string originChamber { get; set; }
        public string originChamberCode { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public DateTime? updateDate { get; set; }
        public DateTime? updateDateIncludingText { get; set; }
        public string url { get; set; }
        public DateTime? introducedDate { get; set; }
        public string constitutionalAuthorityStatementText { get; set; }

        // Stub fields
        public Stub actions { get; set; }
        public Stub amendments { get; set; }
        public Cbocostestimate[] cboCostEstimates { get; set; }
        public Committeereport[] committeeReports { get; set; }
        public Stub committees { get; set; }
        public Stub cosponsors { get; set; }
        public Policyarea policyArea { get; set; }
        public Stub relatedBills { get; set; }
        public Stub subjects { get; set; }
        public Stub summaries { get; set; }
        public Stub textVersions { get; set; }
        public Stub titles { get; set; }

        // Collections to be filled with detailed data
        public Action[] detailedActions { get; set; }
        //public Amendment[] detailedAmendments { get; set; }
        //public Committee[] detailedCommittees { get; set; }
        public Cosponsor[] detailedCosponsors { get; set; }
        public RelatedBill[] detailedRelatedBills { get; set; }
        public LegislativeSubject[] detailedSubjects { get; set; }
        public Summary[] detailedSummaries { get; set; }
        public TextVersion[] detailedTextVersions { get; set; }
        //public Title[] detailedTitles { get; set; }
        public Sponsor[] sponsors { get; set; } // Assuming sponsors are directly available
        public Law[] laws { get; set; } // Assuming laws are directly available
    }

    public class Stub
    {
        public int count { get; set; }
        public string url { get; set; }
    }
    public class Action
    {
        public string actionCode { get; set; }
        public DateTime? actionDate { get; set; }
        public Committee[] committees { get; set; } // Added to include committee details
        public SourceSystem sourceSystem { get; set; }
        public string text { get; set; }
        public string type { get; set; }
        public RecordedVote[] recordedVotes { get; set; } // Assuming this was part of the original design
    }

    // New Committee class to represent committee details within an action
    public class Committee
    {
        public string name { get; set; }
        public string systemCode { get; set; }
        public string url { get; set; }
    }

    // Existing SourceSystem class, assuming it was defined similar to this
    public class SourceSystem
    {
        public int code { get; set; }
        public string name { get; set; }
    }


    public class RecordedVote
    {
        public string chamber { get; set; }
        public int congress { get; set; }
        public DateTime? date { get; set; }
        public int rollNumber { get; set; }
        public int sessionNumber { get; set; }
        public string url { get; set; }
    }

    public class Amendments
    {
        public int count { get; set; }
        public string url { get; set; }
    }

    public class Committees
    {
        public int count { get; set; }
        public string url { get; set; }
    }

    public class Cosponsor
    {
        public string bioguideId { get; set; }
        public int district { get; set; }
        public string firstName { get; set; }
        public string fullName { get; set; }
        public bool isOriginalCosponsor { get; set; }
        public string lastName { get; set; }
        public string middleName { get; set; }
        public string party { get; set; }
        public DateTime? sponsorshipDate { get; set; }
        public string state { get; set; }
        public string url { get; set; }
    }

    public class Policyarea
    {
        public string name { get; set; }
    }

    public class RelatedBill
    {
        public int congress { get; set; }
        public LatestAction latestAction { get; set; }
        public string number { get; set; }
        public RelationshipDetail[] relationshipDetails { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string url { get; set; }
    }

    public class RelationshipDetail
    {
        public string identifiedBy { get; set; }
        public string type { get; set; }
    }

    public class Summary
    {
        public DateTime? actionDate { get; set; }
        public string actionDesc { get; set; }
        public string text { get; set; }
        public DateTime? updateDate { get; set; }
        public string versionCode { get; set; }
    }

    public class Subjects
    {
        public LegislativeSubject[] legislativeSubjects { get; set; }
        public PolicyArea policyArea { get; set; }
    }

    public class LegislativeSubject
    {
        public string name { get; set; }
    }

    public class PolicyArea
    {
        public string name { get; set; }
    }

    public class TextVersion
    {
        public DateTime? date { get; set; } // Nullable to handle null dates
        public Format[] formats { get; set; }
        public string type { get; set; }
    }

    public class Format
    {
        public string type { get; set; }
        public string url { get; set; }
    }

    public class Titles
    {
        public int count { get; set; }
        public string url { get; set; }
    }

    public class Cbocostestimate
    {
        public string description { get; set; }
        public DateTime? pubDate { get; set; }
        public string title { get; set; }
        public string url { get; set; }
    }

    public class Committeereport
    {
        public string citation { get; set; }
        public string url { get; set; }
    }

    public class Law
    {
        public string number { get; set; }
        public string type { get; set; }
    }

    public class Sponsor
    {
        public string bioguideId { get; set; }
        public int district { get; set; }
        public string firstName { get; set; }
        public string fullName { get; set; }
        public string isByRequest { get; set; }
        public string lastName { get; set; }
        public string middleName { get; set; }
        public string party { get; set; }
        public string state { get; set; }
        public string url { get; set; }
    }

    public class TextVersionsResponse
    {
        public TextVersionResponse[] textVersions { get; set; }
        public Pagination pagination { get; set; }
    }

    public class TextVersionResponse
    {
        public string date { get; set; }
        public FormatResponse[] formats { get; set; }
        public string type { get; set; }
    }

    public class FormatResponse
    {
        public string type { get; set; }
        public string url { get; set; }
    }

    public class SummariesResponse
    {
        public SummaryResponse[] summaries { get; set; }
        public Pagination pagination { get; set; }
    }

    public class SummaryResponse
    {
        public string actionDate { get; set; }
        public string actionDesc { get; set; }
        public string text { get; set; }
        public string updateDate { get; set; }
        public string versionCode { get; set; }
    }
    public class SubjectsResponse
    {
        public Subjects subjects { get; set; }
        public Pagination pagination { get; set; }
    }


    public class LegislativeSubjectResponse
    {
        public string name { get; set; }
    }

    public class PolicyAreaResponse
    {
        public string name { get; set; }
    }



    // Update helper classes to include pagination
    public class RelatedBillsResponse
    {
        public RelatedBillResponse[] relatedBills { get; set; }
        public Pagination pagination { get; set; }
    }

    public class Pagination
    {
        public int count { get; set; }
        public string next { get; set; }
        public int countIncludingWithdrawnCosponsors { get; set; }
    }


    public class RelatedBillResponse
    {
        public int congress { get; set; }

        public LatestActionResponse latestAction { get; set; }
        public int number { get; set; }

        public RelationshipDetailResponse[] relationshipDetails { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string url { get; set; }
    }

    public class LatestActionResponse
    {
        public string actionDate { get; set; }

        public string text { get; set; }
    }

    public class RelationshipDetailResponse
    {
        public string identifiedBy { get; set; }

        public string type { get; set; }
    }

    // Helper classes for deserialization
    public class CosponsorsResponse
    {
        public CosponsorResponse[] cosponsors { get; set; }
        public Pagination pagination { get; set; }
    }

    public class CosponsorResponse
    {
        public string bioguideId { get; set; }

        public int district { get; set; }

        public string firstName { get; set; }

        public string fullName { get; set; }

        public bool isOriginalCosponsor { get; set; }

        public string lastName { get; set; }

        public string middleName { get; set; }

        public string party { get; set; }

        public string sponsorshipDate { get; set; }

        public string state { get; set; }

        public string url { get; set; }
    }
    // Helper classes to deserialize the JSON response
    public class ActionsResponse
    {
        public ActionResponse[] actions { get; set; }
        public Pagination pagination { get; set; }
    }
    public class ActionResponse
    {
        public string actionCode { get; set; }
        public string actionDate { get; set; }
        public CommitteeResponse[] committees { get; set; } // Added to match API response
        public SourceSystemResponse sourceSystem { get; set; }
        public string text { get; set; }
        public string type { get; set; }
    }

    public class CommitteeResponse
    {
        public string name { get; set; }

        public string systemCode { get; set; }
        public string url { get; set; }
    }

    public class SourceSystemResponse
    {
        public int code { get; set; }
        public string name { get; set; }
    }
    public class BillsFetchResult
    {
        public List<Bill> Bills { get; set; }
        public FetchState UpdatedState { get; set; }
    }
}

}