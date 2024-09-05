using Microsoft.Azure.Cosmos;
using CongressDataCollector.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Services
{
    public class CosmosDbService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;

        public CosmosDbService(string endpoint, string key, string databaseName, string containerName)
        {
            _cosmosClient = new CosmosClient(endpoint, key);
            _container = _cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task<IEnumerable<Bill>> GetBillsAsync(string queryString)
        {
            var query = _container.GetItemQueryIterator<Bill>(new QueryDefinition(queryString));
            var results = new List<Bill>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.ToList());
            }
            return results;
        }

        public async Task<Bill> GetBillByIdAsync(string id)
        {
            try
            {
                ItemResponse<Bill> response = await _container.ReadItemAsync<Bill>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }
}
