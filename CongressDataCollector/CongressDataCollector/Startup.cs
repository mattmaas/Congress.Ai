using System;
using CongressDataCollector.Core.Interfaces;
using CongressDataCollector.Services;
using CongressDataCollector.Infrastructure;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Cosmos;

[assembly: FunctionsStartup(typeof(CongressDataCollector.Functions.Startup))]

namespace CongressDataCollector.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Register HttpClient as a singleton
            builder.Services.AddHttpClient();

            // Utilize CosmosDbInitializer to register CosmosClient
            builder.Services.AddSingleton(_ => CosmosDbInitializer.InitializeCosmosClient());

            // Utilize CosmosDbInitializer to register Cosmos Container
            builder.Services.AddSingleton(serviceProvider =>
            {
                var cosmosClient = serviceProvider.GetService<CosmosClient>();
                return CosmosDbInitializer.InitializeContainer(cosmosClient);
            });

            // Register services
            builder.Services.AddSingleton<IBillService, BillService>();

            // Register CosmosDbService with DI, using the initialized Container
            builder.Services.AddSingleton<ICosmosDbService>(serviceProvider =>
            {
                var container = serviceProvider.GetService<Container>();
                return new CosmosDbService(container);
            });

            // Register BlobStorageManager
            builder.Services.AddSingleton(_ =>
            {
                var connectionString = Environment.GetEnvironmentVariable("BlobStorageConnection");
                return new BlobStorageManager(connectionString ?? throw new InvalidOperationException("Can't find Blob Storage connection string"));
            });

            // Adjusted StateService registration to use BlobStorageManager
            builder.Services.AddSingleton<IStateService>(serviceProvider =>
            {
                var blobStorageManager = serviceProvider.GetService<BlobStorageManager>();
                return new StateService(blobStorageManager);
            });

            // Register OpenAIService with HttpClient
            builder.Services.AddHttpClient<OpenAiService>();
        }
    }
}
