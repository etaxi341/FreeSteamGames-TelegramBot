using System.Text.Json.Serialization;

namespace SteamCrawler.Models.Steam;

// Not used because Steam return different thing based on the game if is avaible for Windows, Mac and Linux
public class HardwareRequirements
{
    [JsonPropertyName("minimum")]
    public string Minimum { get; set; }

    [JsonPropertyName("recommended")]
    public string Recommended { get; set; }
}