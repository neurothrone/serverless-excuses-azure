using System.Text.Json.Serialization;

namespace ServerlessExcuses.Api.Models;

public class Excuse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("used-count")]
    public int UsedCount { get; set; } = 0;
}