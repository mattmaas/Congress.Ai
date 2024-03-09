using CongressDataCollector.Core.Models;
using Microsoft.Extensions.Logging;

namespace CongressDataCollector.Core.Interfaces;

public interface IBillService
{
    BillsFetchResult FetchBills(FetchState state, ILogger log, DateTime? fromDateTime = null, DateTime? toDateTime = null, string sort = "updateDate+desc");
    Bill FetchBillDetails(Bill bill, FetchState state, ILogger log);

}