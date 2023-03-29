using System.Text.Json.Serialization;

namespace SteamCrawler.Models.Steam;

/// <summary>
/// This class is for the complete game details (if "success" is true, "Data" will have the infos)
/// </summary>
public class GameDetails
{
    [JsonPropertyName("success")]
    public bool Success { get; set; } = false;

    [JsonPropertyName("data")]
    public Data Data { get; set; }
}