using CongressDataCollector.Core.Models;
using Microsoft.Extensions.Logging;

namespace CongressDataCollector.Core.Interfaces;

public interface ICosmosDbService
{
    void StoreBillInCosmosDb(Bill bill, ILogger log);
}