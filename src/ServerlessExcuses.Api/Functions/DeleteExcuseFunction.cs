using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using ServerlessExcuses.Api.Models;

namespace ServerlessExcuses.Api.Functions;

public class DeleteExcuseFunction
{
    private readonly Container _container;

    public DeleteExcuseFunction(CosmosClient cosmosClient)
    {
        _container = cosmosClient.GetContainer("ExcusesDB", "Excuses");
    }

    [Function("DeleteExcuse")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "excuses/{id}")]
        HttpRequestData req,
        string id)
    {
        try
        {
            await _container.DeleteItemAsync<Excuse>(id, new PartitionKey(id));
            var response = req.CreateResponse(HttpStatusCode.NoContent);
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
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Unexpected error: {ex.Message}");
            return errorResponse;
        }
    }
}