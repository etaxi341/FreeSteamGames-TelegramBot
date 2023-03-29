using System.Text.Json.Serialization;

namespace SteamCrawler.Models.Steam;

public class Category
{
    [JsonPropertyName("id")]
    public int ID { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
}