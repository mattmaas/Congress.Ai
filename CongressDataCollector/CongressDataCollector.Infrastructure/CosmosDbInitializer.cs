using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace CongressDataCollector.Infrastructure
{
    public class CosmosDbInitializer
    {
        public static CosmosClient InitializeCosmosClient(IConfiguration configuration)
        {
            var cosmosEndpoint = configuration["CosmosEndpoint"];
            var cosmosKey = configuration["CosmosKey"];
            return new CosmosClient(cosmosEndpoint, cosmosKey);
        }

        public static Container InitializeContainer(CosmosClient cosmosClient, IConfiguration configuration)
        {
            var databaseId = configuration["CosmosDatabaseId"];
            var containerId = configuration["CosmosContainerId"];
            return cosmosClient.GetContainer(databaseId, containerId);
        }
    }
}
