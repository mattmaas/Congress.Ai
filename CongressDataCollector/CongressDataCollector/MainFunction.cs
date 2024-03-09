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
                var allBillsFetchResult = _billService.FetchBills(state, log);
                processedBills.AddRange(allBillsFetchResult.Bills);
            }
            else
            {
                var latestToDate = DateTime.UtcNow;
                var latestFromDate = state.LatestFetchedBillActionDate ?? DateTime.MinValue;
                var latestBillsFetchResult = _billService.FetchBills(state, log, latestFromDate, latestToDate, "updateDate+desc");
                processedBills.AddRange(latestBillsFetchResult.Bills);

                var earliestToDate = state.EarliestFetchedBillActionDate ?? latestFromDate.AddDays(1);
                var earliestFromDate = DateTime.MinValue;
                var earliestBillsFetchResult = _billService.FetchBills(state, log, earliestFromDate, earliestToDate, "updateDate+asc");
                processedBills.AddRange(earliestBillsFetchResult.Bills);
            }
            foreach (var bill in processedBills)
            {
                _billService.FetchBillDetails(bill, state, log);
            }

            UpdateStateDates(processedBills, state);
            await _stateService.SaveStateAsync(state); // Assuming there's a synchronous version of this method

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
