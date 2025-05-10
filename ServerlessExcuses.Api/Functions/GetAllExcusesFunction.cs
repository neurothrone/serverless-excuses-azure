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
        using var iterator = _container.GetItemQueryIterator<Excuse>("SELECT * FROM c");
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            excuses.AddRange(response.Resource);
        }

        var responseData = req.CreateResponse(HttpStatusCode.OK);
        await responseData.WriteAsJsonAsync(excuses);
        return responseData;
    }
}