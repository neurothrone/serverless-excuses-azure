using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

// Add Cosmos DB client
builder.Services.AddSingleton(_ =>
{
    var endpoint = Environment.GetEnvironmentVariable("CosmosDbEndpoint") ??
                   throw new InvalidOperationException("CosmosDbEndpoint is not set.");
    var key = Environment.GetEnvironmentVariable("CosmosDbKey") ??
              throw new InvalidOperationException("CosmosDbKey is not set.");

    var cosmosClientOptions = new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    };

    return new CosmosClient(endpoint, key, cosmosClientOptions);
});

builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();