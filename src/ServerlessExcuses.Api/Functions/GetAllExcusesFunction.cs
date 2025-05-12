using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using ServerlessExcuses.Api.Models;

namespace ServerlessExcuses.Api.Functions;

public class GetAllExcusesFunction
{
    private readonly Container _container;

    public GetAllExcusesFunction(CosmosClient cosmosClient)
    {
        _container = cosmosClient.GetContainer("ExcusesDB", "Excuses");
    }

    [Function("GetAllExcuses")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "excuses")]
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

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(excuses);
            return response;
        }
        catch (Exception ex)
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error retrieving excuses: {ex.Message}");
            return errorResponse;
        }
    }
}