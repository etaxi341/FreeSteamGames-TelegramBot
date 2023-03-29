using System.Text.Json.Serialization;

namespace SteamCrawler.Models.Steam;

/// <summary>
/// This class is for the search results
/// </summary>
public class Capsule
{

    [JsonPropertyName("desc")]
    public string Desc { get; set; }

    [JsonPropertyName("items")]
    public Game[] Games { get; set; }

    /// <summary>List of free games (can be empty)</summary>
    public class Game
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("logo")]
        public string LogoLink { get; set; }
    }
}