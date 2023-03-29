using System.Text.Json.Serialization;
using static SteamCrawler.Steam;

namespace SteamCrawler.Models.Steam;

public class Movie
{
    [JsonPropertyName("id")]
    public int ID { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("thumbnail")]
    public string Thumbnail { get; set; }

    [JsonPropertyName("webm")]
    public Webm Webm { get; set; }

    [JsonPropertyName("mp4")]
    public Mp4 Mp4 { get; set; }

    [JsonPropertyName("highlight")]
    public bool Highlight { get; set; }
}