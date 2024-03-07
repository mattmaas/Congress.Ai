using CongressDataCollector.Core.Models;

namespace CongressDataCollector.Core.Interfaces;

public interface ICosmosDbService
{
    Task StoreBillInCosmosDbAsync(Bill bill);
}