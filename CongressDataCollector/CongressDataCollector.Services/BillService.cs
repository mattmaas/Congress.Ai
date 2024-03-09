using CongressDataCollector.Core.Interfaces;
using CongressDataCollector.Core.Models;
using CongressDataCollector.Core.Models.Responses;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace CongressDataCollector.Services
{
    public class BillService : IBillService
    {
        private readonly IOpenAiService _openAiService;
        private readonly string _apiKey;
        private readonly ICosmosDbService _cosmosDbService;
        private readonly HttpClient _httpClient;

        public BillService(ICosmosDbService cosmosDbService, HttpClient httpClient, IOpenAiService openAiService)
        {
            _apiKey = Environment.GetEnvironmentVariable("CongressAPIKEY") ?? throw new InvalidOperationException("API KEY not found");
            _cosmosDbService = cosmosDbService;
            _httpClient = httpClient;
            _openAiService = openAiService;
        }

        public BillsFetchResult FetchBills(FetchState state, ILogger log, DateTime? fromDateTime = null, DateTime? toDateTime = null, string sort = "updateDate+desc")
        {
            log.LogInformation($"FetchBills function executed at: {DateTime.Now}");
            var fetchedBills = new List<Bill>();

            const int congressNumber = 118;
            var offset = 0;
            const int limit = 250;

            var baseUri = $"https://api.congress.gov/v3/bill/{congressNumber}?format=json&limit={limit}&sort={sort}";
            var requestUri = baseUri + $"&api_key={_apiKey}";

            if (fromDateTime.HasValue)
                requestUri += $"&fromDateTime={fromDateTime.Value:yyyy-MM-ddTHH:mm:ssZ}";
            if (toDateTime.HasValue)
                requestUri += $"&toDateTime={toDateTime.Value:yyyy-MM-ddTHH:mm:ssZ}";

            while (!string.IsNullOrEmpty(requestUri) && fetchedBills.Count < limit)
            {
                var response = SendRequestWithRetry(requestUri, log);
                if (response.IsSuccessStatusCode)
                {
                    var responseData = ParseResponse<RootObject>(response.Content.ReadAsStringAsync().Result);
                    fetchedBills.AddRange(responseData.Bills);
                    offset += limit;
                    requestUri = baseUri + $"&offset={offset}&api_key={_apiKey}";
                }
                else
                {
                    requestUri = null;
                }
                break;
            }

            return new BillsFetchResult { Bills = fetchedBills, UpdatedState = state };
        }

        public Bill FetchBillDetails(Bill bill, FetchState state, ILogger log)
        {
            FetchGeneralBillDetails(bill, log);

            if (bill.HasDetailedInfo)
            {
                if (bill.Actions?.HasStubData == true)
                {
                    log.LogInformation($"Fetching detailed actions for {nameof(bill)} with ID: {bill.Id}");
                    FetchDetailedActions(bill, log);
                    log.LogInformation($"Fetched actions");
                }

                if (bill.Cosponsors?.HasStubData == true)
                {
                    log.LogInformation($"Fetching detailed Cosponsors for {nameof(bill)} with ID: {bill.Id}");
                    FetchDetailedCosponsors(bill, log);
                    log.LogInformation($"Fetched Cosponsors");
                }

                if (bill.RelatedBills?.HasStubData == true)
                {
                    log.LogInformation($"Fetching detailed DetailedRelated for {nameof(bill)} with ID: {bill.Id}");
                    FetchDetailedRelatedBills(bill, log);
                    log.LogInformation($"Fetched DetailedRelated");
                }

                if (bill.Subjects?.HasStubData == true)
                {
                    log.LogInformation($"Fetching detailed Subjects for {nameof(bill)} with ID: {bill.Id}");
                    FetchDetailedSubjects(bill, log);
                    log.LogInformation($"Fetched Subjects");
                }

                if (bill.Summaries?.HasStubData == true)
                {
                    log.LogInformation($"Fetching detailed Summaries for {nameof(bill)} with ID: {bill.Id}");
                    FetchDetailedSummaries(bill, log);
                    log.LogInformation($"Fetched Summaries");
                }

                if(bill.TextVersions?.HasStubData == true)
                {
                    log.LogInformation($"Fetching detailed TextVersions for {nameof(bill)} with ID: {bill.Id}");
                    FetchDetailedTextVersions(bill, log);
                    log.LogInformation($"Fetched TextVersions");
                }

                if (bill.DetailedTextVersions?.Any() == true)
                {
                    log.LogInformation($"Fetching detailed BillText for {nameof(bill)} with ID: {bill.Id}");
                    FetchBillText(bill, log);
                    log.LogInformation($"Fetched BillText");

                    if (!string.IsNullOrEmpty(bill.PlainText))
                    {
                        log.LogInformation($"Fetching OpenAiSummaries for {nameof(bill)} with ID: {bill.Id}");
                        FetchOpenAiSummaries(bill, log);
                        log.LogInformation($"Fetched OpenAiSummaries");
                    }
                }
            }
            log.LogInformation($"Fetched detailed information for {nameof(bill)} with ID: {bill.Id}");

            _cosmosDbService.StoreBillInCosmosDb(bill, log);

            return bill;
        }

        private void FetchGeneralBillDetails(Bill bill, ILogger log)
        {
            var requestUri = $"{bill.Url}&limit=250&api_key={_apiKey}";
            var response = SendRequestWithRetry(requestUri, log);
            var billResponse = ParseResponse<BillResponse>(response.Content.ReadAsStringAsync().Result);
            bill.UpdateWithDetailedInfo(billResponse.Bill);
            bill.HasDetailedInfo = true;
        }

        private void FetchDetailedActions(Bill bill, ILogger log)
        {
            var requestUri = $"{bill.Actions.Url}&limit=250&api_key={_apiKey}";
            var response = SendRequestWithRetry(requestUri, log);
            var actionsResponse = ParseResponse<ActionsResponse>(response.Content.ReadAsStringAsync().Result);
            bill.DetailedActions = actionsResponse.Actions;
        }

        private void FetchDetailedCosponsors(Bill bill, ILogger log)
        {
            var requestUri = $"{bill.Cosponsors.Url}&limit=250&api_key={_apiKey}";
            var response = SendRequestWithRetry(requestUri, log);
            var cosponsorsResponse = ParseResponse<CosponsorsResponse>(response.Content.ReadAsStringAsync().Result);
            bill.DetailedCosponsors = cosponsorsResponse.Cosponsors;
        }

        private void FetchDetailedRelatedBills(Bill bill, ILogger log)
        {
            var requestUri = $"{bill.RelatedBills.Url}&limit=250&api_key={_apiKey}";
            var response = SendRequestWithRetry(requestUri, log);
            var relatedBillsResponse = ParseResponse<RelatedBillsResponse>(response.Content.ReadAsStringAsync().Result);
            bill.DetailedRelatedBills = relatedBillsResponse.RelatedBills;
        }

        private void FetchDetailedSubjects(Bill bill, ILogger log)
        {
            var requestUri = $"{bill.Subjects.Url}&limit=250&api_key={_apiKey}";
            var response = SendRequestWithRetry(requestUri, log);
            var subjectsResponse = ParseResponse<SubjectsResponse>(response.Content.ReadAsStringAsync().Result);
            bill.DetailedSubjects = subjectsResponse.Subjects.LegislativeSubjects;
        }

        private void FetchDetailedSummaries(Bill bill, ILogger log)
        {
            var requestUri = $"{bill.Summaries.Url}&limit=250&api_key={_apiKey}";
            var response = SendRequestWithRetry(requestUri, log);
            var summariesResponse = ParseResponse<SummariesResponse>(response.Content.ReadAsStringAsync().Result);
            bill.DetailedSummaries = summariesResponse.Summaries;
        }

        private void FetchDetailedTextVersions(Bill bill, ILogger log)
        {
            var requestUri = $"{bill.TextVersions.Url}&limit=250&api_key={_apiKey}";
            var response = SendRequestWithRetry(requestUri, log);
            var textVersionsResponse = ParseResponse<TextVersionsResponse>(response.Content.ReadAsStringAsync().Result);
            bill.DetailedTextVersions = textVersionsResponse.TextVersions;
        }
        private void FetchBillText(Bill bill, ILogger log)
        {
            var preferredTextVersion = bill.DetailedTextVersions.Find(tv => tv.Formats.Exists(f => f.Type == "Formatted Text"))?.Formats.Find(f => f.Type == "Formatted Text") ??
                                       bill.DetailedTextVersions.Find(tv => tv.Formats.Count > 0)?.Formats[0];

            if (preferredTextVersion == null) return;

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, preferredTextVersion.Url);
                var response = _httpClient.Send(request, HttpCompletionOption.ResponseContentRead);
                response.EnsureSuccessStatusCode();
                var textContent = response.Content.ReadAsStringAsync().Result; // This is the synchronous part
                bill.PlainText = textContent;
            }
            catch (Exception ex)
            {
                log.LogError($"Error fetching bill text: {ex.Message}");
            }
        }


        private void FetchOpenAiSummaries(Bill bill, ILogger log)
        {
             var analysis = _openAiService.AnalyzeBillTextAsync(bill.PlainText).Result;
            bill.OpenAiSummaries = JsonConvert.DeserializeObject<OpenAiSummaries>(analysis);
        }

        private HttpResponseMessage SendRequestWithRetry(string requestUri, ILogger log, int maxRetries = 20)
        {
            log.LogInformation($"Sending request to URI: {requestUri}");

            HttpResponseMessage response = null;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                response = _httpClient.GetAsync(requestUri).Result;
                if (response.IsSuccessStatusCode){
                    log.LogInformation($"Received successful response from URI: {requestUri}");
                    break;
                }

                if ((int)response.StatusCode == 429) // Too many requests
                {
                    retryCount++;
                    var waitTime = GetRetryAfterTime(response);
                    System.Threading.Thread.Sleep(waitTime * 1000);
                    log.LogError($"Failed to fetch data from URI: {requestUri}. {response}");

                }
                else break;
            }

            return response;
        }

        private static int GetRetryAfterTime(HttpResponseMessage response)
        {
            return response.Headers.RetryAfter?.Delta.HasValue ?? false ? (int)response.Headers.RetryAfter.Delta.Value.TotalSeconds : 3600;
        }

        private static T ParseResponse<T>(string responseContent)
        {
            return JsonConvert.DeserializeObject<T>(responseContent);
        }
    }
}
