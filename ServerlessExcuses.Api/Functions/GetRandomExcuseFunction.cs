using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using ServerlessExcuses.Api.Models;

namespace ServerlessExcuses.Api.Functions;

public class GetRandomExcuseFunction
{
    private readonly Container _container;

    public GetRandomExcuseFunction(CosmosClient cosmosClient)
    {
        _container = cosmosClient.GetContainer("ExcusesDB", "Excuses");
    }

    [Function("GetRandomExcuse")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "excuses/random")]
        HttpRequestData req)
    {
        List<Excuse> excuses = [];

        try
        {
            using var iterator = _container.GetItemQueryIterator<Excuse>("SELECT * FROM c");
            while (iterator.HasMoreResults)
            {
                var feedResponse = await iterator.ReadNextAsync();
                excuses.AddRange(feedResponse.Resource);
            }

            if (excuses.Count == 0)
            {
                var emptyResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await emptyResponse.WriteStringAsync("No excuses found.");
                return emptyResponse;
            }

            var random = new Random();
            var excuse = excuses[random.Next(excuses.Count)];

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(excuse);
            return response;
        }
        catch (Exception ex)
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error retrieving random excuse: {ex.Message}");
            return errorResponse;
        }
    }
}