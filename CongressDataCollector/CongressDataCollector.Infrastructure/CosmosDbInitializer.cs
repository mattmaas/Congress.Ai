using Microsoft.Azure.Cosmos;

namespace CongressDataCollector.Infrastructure
{
    public class CosmosDbInitializer
    {
        public static CosmosClient InitializeCosmosClient()
        {
            var cosmosEndpoint = Environment.GetEnvironmentVariable("CosmosEndpoint");
            var cosmosKey = Environment.GetEnvironmentVariable("CosmosKey");
            return new CosmosClient(cosmosEndpoint, cosmosKey);
        }

        public static Container InitializeContainer(CosmosClient cosmosClient)
        {
            var databaseId = Environment.GetEnvironmentVariable("CosmosDatabaseId");
            var containerId = Environment.GetEnvironmentVariable("CosmosContainerId");
            return cosmosClient.GetContainer(databaseId, containerId);
        }
    }
}