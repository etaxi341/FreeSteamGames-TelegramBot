using System.Text.Json.Serialization;

namespace SteamCrawler.Models.Steam;

public class Recommendations
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
}