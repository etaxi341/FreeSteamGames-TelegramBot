using System.Text.Json.Serialization;

namespace SteamCrawler.Models.Steam;

public class Screenshot
{
    [JsonPropertyName("id")]
    public int ID { get; set; }

    [JsonPropertyName("path_thumbnail")]
    public string PathThumbnail { get; set; }

    [JsonPropertyName("path_full")]
    public string PathFull { get; set; }
}