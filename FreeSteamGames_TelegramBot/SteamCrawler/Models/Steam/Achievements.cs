using System.Text.Json.Serialization;

namespace SteamCrawler.Models.Steam;

public class Achievements
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("highlighted")]
    public Highlighted[] Highlighted { get; set; }
}