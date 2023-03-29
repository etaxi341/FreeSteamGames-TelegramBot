using System.Text.Json.Serialization;

namespace SteamCrawler.Models.Steam;

public class Genre
{
    [JsonPropertyName("id")]
    public string ID { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
}