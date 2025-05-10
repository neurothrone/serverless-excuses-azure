using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using ServerlessExcuses.Api.Models;

namespace ServerlessExcuses.Api.Functions;

public class GetExcuseByIdFunction
{
    private readonly Container _container;

    public GetExcuseByIdFunction(CosmosClient cosmosClient)
    {
        _container = cosmosClient.GetContainer("ExcusesDB", "Excuses");
    }

    [Function("GetExcuseById")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "excuses/{id}")]
        HttpRequestData req,
        string id)
    {
        try
        {
            var item = await _container.ReadItemAsync<Excuse>(id, new PartitionKey(id));
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(item.Resource);
            return response;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync("Excuse not found.");
            return notFound;
        }
        catch (Exception ex)
        {
            var error = req.CreateResponse(HttpStatusCode.InternalServerError);
            await error.WriteStringAsync($"Unexpected error: {ex.Message}");
            return error;
        }
    }
}