using System.Net;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using ServerlessExcuses.Api.Models;

namespace ServerlessExcuses.Api.Functions;

public class CreateExcuseFunction
{
    private readonly Container _container;

    public CreateExcuseFunction(CosmosClient cosmosClient)
    {
        _container = cosmosClient.GetContainer("ExcusesDB", "Excuses");
    }

    [Function("CreateExcuse")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "excuses")]
        HttpRequestData req)
    {
        try
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var requestBody = JsonSerializer.Deserialize<CreateExcuseDto>(
                body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (requestBody is null || string.IsNullOrWhiteSpace(requestBody.Text))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Missing or invalid 'text' field.");
                return badRequest;
            }

            var excuse = new Excuse
            {
                Id = Guid.NewGuid().ToString(),
                Text = requestBody.Text,
                UsedCount = 0
            };

            // await _container.CreateItemAsync(excuse, new PartitionKey(excuse.Id));
            var itemResponse = await _container.CreateItemAsync(excuse, new PartitionKey(excuse.Id));
            Console.WriteLine($"Cosmos insert successful. RU charge: {itemResponse.RequestCharge}");

            var response = req.CreateResponse(HttpStatusCode.Created);

            await response.WriteAsJsonAsync(excuse);
            return response;
        }
        catch (CosmosException ex)
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Cosmos DB error: {ex.Message}");
            return errorResponse;
        }
        catch (Exception ex)
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Unexpected error: {ex.Message}");
            return errorResponse;
        }
    }
}