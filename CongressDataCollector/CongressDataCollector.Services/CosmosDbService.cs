using CongressDataCollector.Core.Interfaces;
using CongressDataCollector.Core.Models;
using Microsoft.Azure.Cosmos;

namespace CongressDataCollector.Services;

public class CosmosDbService : ICosmosDbService
{
    private readonly Container _container;

    public CosmosDbService(Container container)
    {
        // Get reference to database and container
        _container = container;
    }

    public async Task StoreBillInCosmosDbAsync(Bill bill)
    {
        try
        {
            // Serialize and store the bill data in Cosmos DB
            await _container.UpsertItemAsync(bill, new PartitionKey(bill.Id));
            Console.WriteLine($"Stored bill {bill.Id} in Cosmos DB.");
        }
        catch (CosmosException ex)
        {
            Console.WriteLine($"Failed to store bill {bill.Id} in Cosmos DB: {ex.Message}");
        }
    }
}