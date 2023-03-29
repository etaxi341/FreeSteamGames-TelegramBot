using System.Text.Json.Serialization;

namespace SteamCrawler.Models.Steam;

public class SupportInfo
{
    [JsonPropertyName("url")]
    public string URL { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }
}