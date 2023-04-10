using System.Reflection;
using System.Text.Json.Serialization;

namespace SellSn.Client.Auth.Models;

public class VersionData
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0.0";
}