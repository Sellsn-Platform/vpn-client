using System.Text.Json.Serialization;

namespace SellSn.Client.Auth.Models;

public class OAuthUrlResponse
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}