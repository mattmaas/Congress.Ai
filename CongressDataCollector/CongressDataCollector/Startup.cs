using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Microsoft.Azure.Cosmos;
using System;

[assembly: FunctionsStartup(typeof(CongressDataCollector.Startup))]

namespace CongressDataCollector
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Register HttpClient as a singleton
            builder.Services.AddHttpClient();

            // Register CosmosClient or any other services
            builder.Services.AddSingleton((s) =>
            {
                var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnection");
                return new CosmosClient(connectionString);
            });

            // Add more services as needed
        }
    }
}
