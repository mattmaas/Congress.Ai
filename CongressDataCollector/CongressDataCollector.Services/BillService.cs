using CongressDataCollector.Core.Interfaces;
using CongressDataCollector.Core.Models;
using CongressDataCollector.Core.Models.Responses;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CongressDataCollector.Services
{
    public class BillService : IBillService
    {
        private readonly string _apiKey;
        private readonly ICosmosDbService _cosmosDbService;
        private readonly HttpClient _httpClient;

        public BillService(ICosmosDbService cosmosDbService, HttpClient httpClient)
        {
            _apiKey = Environment.GetEnvironmentVariable("CongressAPIKEY") ?? throw new InvalidOperationException("API KEY not found");
            _cosmosDbService = cosmosDbService;
            _httpClient = httpClient;
        }

        public async Task<BillsFetchResult> FetchBillsAsync(FetchState state, ILogger log, DateTime? fromDateTime = null, DateTime? toDateTime = null, string sort = "updateDate+desc")
        {
            log.LogInformation($"FetchBillsAsync function executed at: {DateTime.Now}");
            var fetchedBills = new List<Bill>();

            const int congressNumber = 118; // Example congress number
            var offset = 0; // Start at the first record
            const int limit = 250; // Maximum records per request

            var baseUri = $"https://api.congress.gov/v3/bill/{congressNumber}?format=json&limit={limit}&sort={sort}";

            var requestUri = baseUri;
            if (fromDateTime.HasValue)
            {
                var fromDateTimeStr = fromDateTime.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");
                requestUri += $"&fromDateTime={fromDateTimeStr}";
            }
            if (toDateTime.HasValue)
            {
                var toDateTimeStr = toDateTime.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");
                requestUri += $"&toDateTime={toDateTimeStr}";
            }
            requestUri += $"&api_key={_apiKey}";

            while (!string.IsNullOrEmpty(requestUri) && fetchedBills.Count < 250)
            {
                var response = await SendRequestWithRetryAsync(requestUri, log);
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await ParseResponseAsync<RootObject>(response);
                    if (responseData?.Bills != null && responseData.Bills.Length > 0)
                    {
                        fetchedBills.AddRange(responseData.Bills);
                        log.LogInformation($"Fetched {responseData.Bills.Length} bills. Total fetched: {fetchedBills.Count}/{responseData.Pagination.Count}");

                        offset += limit; // Move to the next set of records
                        requestUri = baseUri + $"&offset={offset}&api_key={_apiKey}";
                        if (fromDateTime.HasValue)
                        {
                            requestUri += $"&fromDateTime={fromDateTime.Value:yyyy-MM-ddTHH:mm:ssZ}";
                        }
                        if (toDateTime.HasValue)
                        {
                            requestUri += $"&toDateTime={toDateTime.Value:yyyy-MM-ddTHH:mm:ssZ}";
                        }
                    }
                    else
                    {
                        log.LogInformation("No more bills to fetch.");
                        break;
                    }
                }
                else
                {
                    log.LogError($"Failed to fetch bills. Status: {response.StatusCode}. Exiting fetch loop.");
                    break;
                }
            }

            return new BillsFetchResult
            {
                Bills = fetchedBills,
                UpdatedState = state
            };
        }
        public async Task<Bill> FetchBillDetailsAsync(Bill bill, FetchState state, ILogger log)
        {
            await FetchGeneralBillDetailsAsync(bill, log);
            if (bill.HasDetailedInfo) // Assuming 'HasDetailedInfo' is a property you add to 'Bill' to indicate that general details were successfully fetched
            {
                // Proceed with fetching additional details only if general details are fetched successfully
                await FetchDetailedActionsAsync(bill, log);
                await FetchDetailedCosponsorsAsync(bill, log);
                await FetchDetailedRelatedBillsAsync(bill, log);
                await FetchDetailedSubjectsAsync(bill, log);
                await FetchDetailedSummariesAsync(bill, log);
                await FetchDetailedTextVersionsAsync(bill, log);
                // Repeat for other detailed fetch methods...
            }
            await _cosmosDbService.StoreBillInCosmosDbAsync(bill);

            return bill;
        }

        private async Task FetchGeneralBillDetailsAsync(Bill bill, ILogger log)
        {
            if (bill == null || string.IsNullOrEmpty(bill.Url)) return;

            var requestUri = $"{bill.Url}&limit=250&api_key={_apiKey}";

            var response = await SendRequestWithRetryAsync(requestUri, log);

            if (!response.IsSuccessStatusCode) return;
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

        private async Task FetchDetailedActionsAsync(Bill bill, ILogger log)
        {
            if (!bill.Actions.HasStubData || bill?.Actions == null || bill.Actions.Count == 0 || string.IsNullOrEmpty(bill.Actions.Url)) return;

            var detailedActions = new List<LegislativeAction>();
            var requestUri = $"{bill.Actions.Url}&limit=250&api_key={_apiKey}";

            while (!string.IsNullOrEmpty(requestUri))
            {
                var response = await SendRequestWithRetryAsync(requestUri, log);

                if (response.IsSuccessStatusCode)
                {
                    var actionsResponse = await ParseResponseAsync<ActionsResponse>(response);
                    detailedActions.AddRange(actionsResponse.Actions);
                    log.LogInformation($"Fetched detailed actions for {bill.Number}.");
                    requestUri = $"{actionsResponse.Pagination.Next}&api_key={_apiKey}";
                }
                else
                {
                    log.LogError($"Failed to fetch detailed actions for {bill.Number}. Status: {response.StatusCode}");
                    break;
                }
            }

            bill.DetailedActions = detailedActions; // Update the bill's detailed actions outside the loop
        }
        private async Task FetchDetailedCosponsorsAsync(Bill bill, ILogger log)
        {
            if (bill?.Cosponsors == null || bill.Cosponsors.Count == 0) return;

            var detailedCosponsors = new List<Cosponsor>();
            var requestUri = $"{bill.Cosponsors.Url}&limit=250&api_key={_apiKey}";

            while (!string.IsNullOrEmpty(requestUri))
            {
                var response = await SendRequestWithRetryAsync(requestUri, log);

                if (response.IsSuccessStatusCode)
                {
                    var cosponsorsResponse = await ParseResponseAsync<CosponsorsResponse>(response);
                    detailedCosponsors.AddRange(cosponsorsResponse.Cosponsors);
                    log.LogInformation($"Fetched detailed cosponsors for {bill.Number}.");
                    requestUri = $"{cosponsorsResponse.Pagination.Next}&api_key={_apiKey}";
                }
                else
                {
                    log.LogError($"Failed to fetch detailed cosponsors for {bill.Number}. Status: {response.StatusCode}");
                    break;
                }
            }

            bill.DetailedCosponsors = detailedCosponsors; // Update the bill's detailed cosponsors outside the loop
        }

        private async Task FetchDetailedRelatedBillsAsync(Bill bill, ILogger log)
        {
            if (bill?.RelatedBills == null || bill.RelatedBills.Count == 0) return;

            var detailedRelatedBills = new List<RelatedBill>();
            var requestUri = $"{bill.RelatedBills.Url}&limit=250&api_key={_apiKey}";

            while (!string.IsNullOrEmpty(requestUri))
            {
                var response = await SendRequestWithRetryAsync(requestUri, log);

                if (response.IsSuccessStatusCode)
                {
                    var relatedBillsResponse = await ParseResponseAsync<RelatedBillsResponse>(response);
                    detailedRelatedBills.AddRange(relatedBillsResponse.RelatedBills);
                    log.LogInformation($"Fetched detailed related bills for {bill.Number}.");
                    requestUri = $"{relatedBillsResponse.Pagination.Next}&api_key={_apiKey}";
                }
                else
                {
                    log.LogError($"Failed to fetch detailed related bills for {bill.Number}. Status: {response.StatusCode}");
                    break;
                }
            }

            bill.DetailedRelatedBills = detailedRelatedBills; // Update the bill's detailed related bills outside the loop
        }

        private async Task FetchDetailedSubjectsAsync(Bill bill, ILogger log)
        {
            if (bill?.Subjects == null || bill.Subjects.Count == 0) return;

            var detailedSubjects = new List<LegislativeSubject>();
            var requestUri = $"{bill.Subjects.Url}&limit=250&api_key={_apiKey}";

            while (!string.IsNullOrEmpty(requestUri))
            {
                var response = await SendRequestWithRetryAsync(requestUri, log);

                if (response.IsSuccessStatusCode)
                {
                    var subjectsResponse = await ParseResponseAsync<SubjectsResponse>(response);
                    detailedSubjects.AddRange(subjectsResponse.Subjects.LegislativeSubjects);
                    log.LogInformation($"Fetched detailed subjects for {bill.Number}.");
                    requestUri = $"{subjectsResponse.Pagination.Next}&api_key={_apiKey}";
                }
                else
                {
                    log.LogError($"Failed to fetch detailed subjects for {bill.Number}. Status: {response.StatusCode}");
                    break;
                }
            }

            bill.DetailedSubjects = detailedSubjects; // Update the bill's detailed subjects outside the loop
        }
        private async Task FetchDetailedSummariesAsync(Bill bill, ILogger log)
        {
            if (bill?.Summaries == null || bill.Summaries.Count == 0) return;

            var detailedSummaries = new List<Summary>();
            var requestUri = $"{bill.Summaries.Url}&limit=250&api_key={_apiKey}";

            while (!string.IsNullOrEmpty(requestUri))
            {
                var response = await SendRequestWithRetryAsync(requestUri, log);

                if (response.IsSuccessStatusCode)
                {
                    var summariesResponse = await ParseResponseAsync<SummariesResponse>(response);
                    detailedSummaries.AddRange(summariesResponse.Summaries);
                    log.LogInformation($"Fetched detailed summaries for {bill.Number}.");
                    requestUri = $"{summariesResponse.Pagination.Next}&api_key={_apiKey}";
                }
                else
                {
                    log.LogError($"Failed to fetch detailed summaries for {bill.Number}. Status: {response.StatusCode}");
                    break;
                }
            }

            bill.DetailedSummaries = detailedSummaries; // Update the bill's detailed summaries outside the loop
        }

        private async Task FetchDetailedTextVersionsAsync(Bill bill, ILogger log)
        {
            if (bill?.TextVersions == null || bill.TextVersions.Count == 0) return;

            var detailedTextVersions = new List<TextVersion>();
            var requestUri = $"{bill.TextVersions.Url}&limit=250&api_key={_apiKey}";

            while (!string.IsNullOrEmpty(requestUri))
            {
                var response = await SendRequestWithRetryAsync(requestUri, log);

                if (response.IsSuccessStatusCode)
                {
                    var textVersionsResponse = await ParseResponseAsync<TextVersionsResponse>(response);
                    detailedTextVersions.AddRange(textVersionsResponse.TextVersions);
                    log.LogInformation($"Fetched detailed text versions for {bill.Number}.");
                    requestUri = $"{textVersionsResponse.Pagination.Next}&api_key={_apiKey}";
                }
                else
                {
                    log.LogError($"Failed to fetch detailed text versions for {bill.Number}. Status: {response.StatusCode}");
                    break;
                }
            }

            bill.DetailedTextVersions = detailedTextVersions; // Update the bill's detailed text versions outside the loop
        }

        private async Task<HttpResponseMessage> SendRequestWithRetryAsync(string requestUri, ILogger log, int maxRetries = 3)
        {
            HttpResponseMessage response = null;
            var retryCount = 0;

            while (retryCount < maxRetries)
            {
                response = await _httpClient.GetAsync(requestUri);
                if (response.IsSuccessStatusCode)
                {
                    break;
                }
                else if ((int)response.StatusCode == 429) // Too many requests
                {
                    retryCount++;
                    var waitTime = GetRetryAfterTime(response);
                    log.LogWarning($"Rate limit hit, retrying in {waitTime} seconds. Attempt {retryCount}.");
                    await Task.Delay(waitTime * 1000);
                }
                else
                {
                    log.LogError($"Request failed with status code {response.StatusCode}.");
                    break;
                }
            }

            return response;
        }

        private static int GetRetryAfterTime(HttpResponseMessage response)
        {
            return response.Headers.RetryAfter?.Delta.HasValue ?? false ? (int)response.Headers.RetryAfter.Delta.Value.TotalSeconds : 3600;
        }

        private static async Task<T> ParseResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
