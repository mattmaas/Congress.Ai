using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CongressDataCollector.Models;
using System.Linq;

namespace CongressDataCollector
{
    public class MainFunction
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public MainFunction(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = Environment.GetEnvironmentVariable("CongressAPIKEY");
        }

        [FunctionName("MainFunction")]
        public async Task RunAsync([TimerTrigger("* * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var state = new FetchState();
            var billFetchResult = await FetchBillsAsync(state, log);

            foreach (var bill in billFetchResult.Bills)
            {
                await FetchBillDetailsAsync(bill, state, log);
            }

            log.LogInformation("Processing complete!");
        }
        public async Task<BillsFetchResult> FetchBillsAsync(FetchState state, ILogger log)
        {
            log.LogInformation($"FetchBillsAsync function executed at: {DateTime.Now}");
            var fetchedBills = new List<Bill>();
            string baseUri = "https://api.congress.gov/v3/bill/118?sort=updateDate+desc&limit=250";
            string requestUri = state.NextRequestUri ?? $"{baseUri}&api_key={_apiKey}";

            while (!string.IsNullOrEmpty(requestUri) && fetchedBills.Count < 750)
            {
                var (response, updatedState) = await SendRequestWithRetryAsync(requestUri, state, log);
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await ParseResponseAsync<RootObject>(response);
                    if (responseData?.Bills != null && responseData.Bills.Any())
                    {
                        fetchedBills.AddRange(responseData.Bills);
                        log.LogInformation($"Fetched {responseData.Bills.Length} bills. Total fetched: {fetchedBills.Count}/{responseData.Pagination.Count}");

                        // Update state with last successful fetch details
                        state = updatedState;
                        state.LastSuccessfulUri = requestUri;

                        // Prepare the next request URI based on pagination info
                        requestUri = responseData.Pagination.Next != null ? $"{responseData.Pagination.Next}&api_key={_apiKey}" : null;
                    }
                    else
                    {
                        log.LogInformation("No more bills to fetch.");
                        break; // Exit the loop if no bills are returned in the response
                    }
                }
                else
                {
                    log.LogError($"Failed to fetch bills. Status: {response.StatusCode}. Exiting fetch loop.");
                    break; // Exit the loop on failure to avoid infinite loop
                }
            }


            return new BillsFetchResult
            {
                Bills = fetchedBills,
                UpdatedState = state
            };
        }

        private async Task<FetchState> FetchBillDetailsAsync(Bill bill, FetchState state, ILogger log)
        {
            state = await FetchGeneralBillDetailsAsync(bill, state, log);

            if (bill.Actions?.Count > 0)
                state = await FetchDetailedActionsAsync(bill, state, log);

            if (bill.Cosponsors?.Count > 0)
                state = await FetchDetailedCosponsorsAsync(bill, state, log);

            if (bill.RelatedBills?.Count > 0)
                state = await FetchDetailedRelatedBillsAsync(bill, state, log);

            if (bill.Subjects?.Count > 0)
                state = await FetchDetailedSubjectsAsync(bill, state, log);

            if (bill.Summaries?.Count > 0)
                state = await FetchDetailedSummariesAsync(bill, state, log);

            if (bill.TextVersions?.Count > 0)
                state = await FetchDetailedTextVersionsAsync(bill, state, log);

            return state;
        }

        private async Task<FetchState> FetchGeneralBillDetailsAsync(Bill bill, FetchState state, ILogger log)
        {
            if (bill == null || string.IsNullOrEmpty(bill.Url)) return state;

            string baseUri = $"{bill.Url}&limit=250";
            string requestUri = state.NextRequestUri ?? $"{baseUri}&api_key={_apiKey}";

            var (response, updatedState) = await SendRequestWithRetryAsync(requestUri, state, log);

            if (response.IsSuccessStatusCode)
            {
                var billResponse = await ParseResponseAsync<BillResponse>(response);

                if (billResponse?.Bill != null)
                {
                    bill.UpdateWithDetailedInfo(billResponse.Bill);
                    log.LogInformation($"Fetched and updated general bill details for {bill.Number}.");
                }
                else
                {
                    log.LogError($"Failed to fetch general bill details for {bill.Number}. Status: {response.StatusCode}");
                }
            }

            return updatedState;
        }

        private async Task<FetchState> FetchDetailedActionsAsync(Bill bill, FetchState state, ILogger log)
        {
            if (bill == null || bill.Actions == null || bill.Actions.Count == 0) return state;

            string baseUri = $"{bill.Actions.Url}&limit=250";
            string requestUri = state.NextRequestUri ?? $"{baseUri}&api_key={_apiKey}";
            var (response, updatedState) = await SendRequestWithRetryAsync(requestUri, state, log);

            if (response.IsSuccessStatusCode)
            {
                var actionsResponse = await ParseResponseAsync<ActionsResponse>(response);
                bill.DetailedActions = actionsResponse?.Actions ?? new List<LegislativeAction>();
                log.LogInformation($"Fetched detailed actions for {bill.Number}.");
            }
            else
            {
                log.LogError($"Failed to fetch detailed actions for {bill.Number}. Status: {response.StatusCode}");
            }

            return updatedState;
        }

        // Continue with FetchDetailedCosponsorsAsync, FetchDetailedRelatedBillsAsync, FetchDetailedSubjectsAsync, FetchDetailedSummariesAsync, and FetchDetailedTextVersionsAsync implementations...
        // Continuing with FetchDetailedCosponsorsAsync...
        private async Task<FetchState> FetchDetailedCosponsorsAsync(Bill bill, FetchState state, ILogger log)
        {
            if (bill == null || bill.Cosponsors == null || bill.Cosponsors.Count == 0) return state;

            string baseUri = $"{bill.Cosponsors.Url}&limit=250";
            string requestUri = state.NextRequestUri ?? $"{baseUri}&api_key={_apiKey}";
            var (response, updatedState) = await SendRequestWithRetryAsync(requestUri, state, log);

            if (response.IsSuccessStatusCode)
            {
                var cosponsorsResponse = await ParseResponseAsync<CosponsorsResponse>(response);
                bill.DetailedCosponsors = cosponsorsResponse?.Cosponsors ?? new List<Cosponsor>();
                log.LogInformation($"Fetched detailed cosponsors for {bill.Number}.");
            }
            else
            {
                log.LogError($"Failed to fetch detailed cosponsors for {bill.Number}. Status: {response.StatusCode}");
            }

            return updatedState;
        }

        private async Task<FetchState> FetchDetailedRelatedBillsAsync(Bill bill, FetchState state, ILogger log)
        {
            if (bill == null || bill.RelatedBills == null || bill.RelatedBills.Count == 0) return state;

            string baseUri = $"{bill.RelatedBills.Url}&limit=250";
            string requestUri = state.NextRequestUri ?? $"{baseUri}&api_key={_apiKey}";
            var (response, updatedState) = await SendRequestWithRetryAsync(requestUri, state, log);

            if (response.IsSuccessStatusCode)
            {
                var relatedBillsResponse = await ParseResponseAsync<RelatedBillsResponse>(response);
                bill.DetailedRelatedBills = relatedBillsResponse?.RelatedBills ?? new List<RelatedBill>();
                log.LogInformation($"Fetched detailed related bills for {bill.Number}.");
            }
            else
            {
                log.LogError($"Failed to fetch detailed related bills for {bill.Number}. Status: {response.StatusCode}");
            }

            return updatedState;
        }
        private async Task<FetchState> FetchDetailedSubjectsAsync(Bill bill, FetchState state, ILogger log)
        {
            if (bill == null || bill.Subjects == null || bill.Subjects.Count == 0) return state;

            string baseUri = $"{bill.Subjects.Url}&limit=250";
            string requestUri = state.NextRequestUri ?? $"{baseUri}&api_key={_apiKey}";
            var (response, updatedState) = await SendRequestWithRetryAsync(requestUri, state, log);

            if (response.IsSuccessStatusCode)
            {
                var subjectsResponse = await ParseResponseAsync<SubjectsResponse>(response);
                // Access LegislativeSubjects from the SubjectsContainer
                bill.DetailedSubjects = subjectsResponse?.Subjects.LegislativeSubjects ?? new List<LegislativeSubject>();
                log.LogInformation($"Fetched detailed subjects for {bill.Number}.");
            }
            else
            {
                log.LogError($"Failed to fetch detailed subjects for {bill.Number}. Status: {response.StatusCode}");
            }

            return updatedState;
        }



        private async Task<FetchState> FetchDetailedSummariesAsync(Bill bill, FetchState state, ILogger log)
        {
            if (bill == null || bill.Summaries == null || bill.Summaries.Count == 0) return state;

            string baseUri = $"{bill.Summaries.Url}&limit=250";
            string requestUri = state.NextRequestUri ?? $"{baseUri}&api_key={_apiKey}";
            var (response, updatedState) = await SendRequestWithRetryAsync(requestUri, state, log);

            if (response.IsSuccessStatusCode)
            {
                var summariesResponse = await ParseResponseAsync<SummariesResponse>(response);
                bill.DetailedSummaries = summariesResponse?.Summaries ?? new List<Summary>();
                log.LogInformation($"Fetched detailed summaries for {bill.Number}.");
            }
            else
            {
                log.LogError($"Failed to fetch detailed summaries for {bill.Number}. Status: {response.StatusCode}");
            }

            return updatedState;
        }

        private async Task<FetchState> FetchDetailedTextVersionsAsync(Bill bill, FetchState state, ILogger log)
        {
            if (bill == null || bill.TextVersions == null || bill.TextVersions.Count == 0) return state;

            string baseUri = $"{bill.TextVersions.Url}&limit=250";
            string requestUri = state.NextRequestUri ?? $"{baseUri}&api_key={_apiKey}";
            var (response, updatedState) = await SendRequestWithRetryAsync(requestUri, state, log);

            if (response.IsSuccessStatusCode)
            {
                var textVersionsResponse = await ParseResponseAsync<TextVersionsResponse>(response);
                bill.DetailedTextVersions = textVersionsResponse?.TextVersions ?? new List<TextVersion>();
                log.LogInformation($"Fetched detailed text versions for {bill.Number}.");
            }
            else
            {
                log.LogError($"Failed to fetch detailed text versions for {bill.Number}. Status: {response.StatusCode}");
            }

            return updatedState;
        }

        private async Task<(HttpResponseMessage, FetchState)> SendRequestWithRetryAsync(string requestUri, FetchState state, ILogger log, int maxRetries = 3)
        {
            HttpResponseMessage response = null;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                response = await _httpClient.GetAsync(requestUri);
                if (response.IsSuccessStatusCode)
                {
                    state.LastSuccessfulUri = requestUri;
                    break;
                }
                else if ((int)response.StatusCode == 429)
                {
                    retryCount++;
                    var waitTime = GetRetryAfterTime(response);
                    log.LogWarning($"Rate limit hit, retrying in {waitTime} seconds. Attempt {retryCount}.");
                    await Task.Delay(waitTime * 1000);
                    state.RateLimitHitCount++;
                }
                else
                {
                    log.LogError($"Request failed with status code {response.StatusCode}.");
                    break;
                }
            }

            return (response, state);
        }

        private int GetRetryAfterTime(HttpResponseMessage response)
        {
            return response.Headers.RetryAfter.Delta.HasValue ? (int)response.Headers.RetryAfter.Delta.Value.TotalSeconds : 60;
        }

        private async Task<T> ParseResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }
    }

    // Define other classes like FetchState, Bill, Action, Cosponsor, etc., as required
}
