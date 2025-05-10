using System.Net;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using ServerlessExcuses.Api.Models;

namespace ServerlessExcuses.Api.Functions;

public class UpdateExcuseFunction
{
    private readonly Container _container;

    public UpdateExcuseFunction(CosmosClient cosmosClient)
    {
        _container = cosmosClient.GetContainer("ExcusesDB", "Excuses");
    }

    [Function("UpdateExcuse")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "excuses/{id}")]
        HttpRequestData req,
        string id)
    {
        try
        {
            var updatedExcuse = await JsonSerializer.DeserializeAsync<Excuse>(req.Body);
            if (updatedExcuse is null || string.IsNullOrWhiteSpace(updatedExcuse.Text))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid excuse data.");
                return badRequest;
            }

            updatedExcuse.Id = id;
            var result = await _container.ReplaceItemAsync(updatedExcuse, id, new PartitionKey(id));

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result.Resource);
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