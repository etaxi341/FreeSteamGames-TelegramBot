using System.Text.Json.Serialization;

namespace SteamCrawler.Models.Steam;

public class Highlighted
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("path")]
    public string Path { get; set; }
}