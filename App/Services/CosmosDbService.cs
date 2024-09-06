using Microsoft.Azure.Cosmos;
using App.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace App.Services
{
    public class CosmosDbService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;

        public CosmosDbService(IConfiguration configuration)
        {
            var endpoint = configuration["CosmosEndpoint"];
            var key = configuration["CosmosKey"];
            var databaseName = configuration["CosmosDatabaseId"];
            var containerName = configuration["CosmosContainerId"];

            _cosmosClient = new CosmosClient(endpoint, key, new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway });
            _container = _cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task<IEnumerable<Bill>> GetBillsAsync(string billType, int pageSize, int offset)
        {
            var queryString = $"SELECT * FROM c WHERE c.type = @billType ORDER BY c.updateDate DESC OFFSET @offset LIMIT @pageSize";
            var queryDefinition = new QueryDefinition(queryString)
                .WithParameter("@billType", billType)
                .WithParameter("@offset", offset)
                .WithParameter("@pageSize", pageSize);

            var query = _container.GetItemQueryIterator<Bill>(queryDefinition);
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
