using System.Text.Json.Serialization;

namespace SteamCrawler.Models.Steam;

public class Webm
{
    [JsonPropertyName("480")]
    public string Px480 { get; set; }

    [JsonPropertyName("max")]
    public string Max { get; set; }
}