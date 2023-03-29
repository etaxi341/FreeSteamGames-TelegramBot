using System.Text.Json.Serialization;

namespace SteamCrawler.Models.Steam;

public class Metacritic
{
    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("url")]
    public string URL { get; set; }
}