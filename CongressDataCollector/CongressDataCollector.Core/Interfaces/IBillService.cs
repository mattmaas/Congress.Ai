using CongressDataCollector.Core.Models;
using Microsoft.Extensions.Logging;

namespace CongressDataCollector.Core.Interfaces;

public interface IBillService
{
    Task<BillsFetchResult> FetchBillsAsync(FetchState state, ILogger log, DateTime? fromDateTime = null, DateTime? toDateTime = null, string sort = "updateDate+desc");
    Task<Bill> FetchBillDetailsAsync(Bill bill, FetchState state, ILogger log);
    Task FetchBillTextAsync(Bill bill, ILogger log, Func<Task> onSuccessCallback);

}