using System.Text.Json.Serialization;

namespace ServerlessExcuses.Api.Models;

public class CreateExcuseDto
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}