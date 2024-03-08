using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CongressDataCollector.Core.Interfaces;
using CongressDataCollector.Core.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace CongressDataCollector.Functions
{
    public class MainFunction
    {
        private readonly IStateService _stateService;
        private readonly IBillService _billService;

        public MainFunction(IStateService stateService, IBillService billService)
        {
            _stateService = stateService;
            _billService = billService;
        }

        [FunctionName("MainFunction")]
        public async Task RunAsync([TimerTrigger("* * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var state = await _stateService.LoadStateAsync() ?? new FetchState();

            var processedBills = new List<Bill>();

            if (!state.LatestFetchedBillActionDate.HasValue || !state.EarliestFetchedBillActionDate.HasValue)
            {
                var allBillsFetchResult = await _billService.FetchBillsAsync(state, log);
                processedBills.AddRange(allBillsFetchResult.Bills.Take(10));
            }
            else
            {
                var latestToDate = DateTime.UtcNow;
                var latestFromDate = state.LatestFetchedBillActionDate ?? DateTime.MinValue;
                var latestBillsFetchResult = await _billService.FetchBillsAsync(state, log, latestFromDate, latestToDate, "updateDate+desc");
                processedBills.AddRange(latestBillsFetchResult.Bills);

                var earliestToDate = state.EarliestFetchedBillActionDate ?? latestFromDate.AddDays(1);
                var earliestFromDate = DateTime.MinValue;
                var earliestBillsFetchResult = await _billService.FetchBillsAsync(state, log, earliestFromDate, earliestToDate, "updateDate+asc");
                processedBills.AddRange(earliestBillsFetchResult.Bills);
            }
            var fetchDetailsTasks = processedBills.ConvertAll(bill => _billService.FetchBillDetailsAsync(bill, state, log));
            await Task.WhenAll(fetchDetailsTasks);

            UpdateStateDates(processedBills, state);
            await _stateService.SaveStateAsync(state);

            log.LogInformation("Processing complete!");
        }

        private static void UpdateStateDates(IEnumerable<Bill> bills, FetchState state)
        {
            var actionDates = bills
                .Where(b => b.LatestAction?.ActionDate != null)
                .Select(b => b.LatestAction.ActionDate.Value)
                .ToList();

            if (!actionDates.Any()) return;
            var earliestDate = actionDates.Min();
            var latestDate = actionDates.Max();

            state.EarliestFetchedBillActionDate = state.EarliestFetchedBillActionDate < earliestDate
                ? state.EarliestFetchedBillActionDate
                : earliestDate;

            state.LatestFetchedBillActionDate = state.LatestFetchedBillActionDate > latestDate
                ? state.LatestFetchedBillActionDate
                : latestDate;
        }
    }
}
