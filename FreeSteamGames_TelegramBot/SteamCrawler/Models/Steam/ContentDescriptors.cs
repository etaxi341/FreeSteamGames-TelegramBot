using System.Text.Json.Serialization;

namespace SteamCrawler.Models.Steam;

public class ContentDescriptors
{
    [JsonPropertyName("ids")]
    public object[] IDs { get; set; }

    [JsonPropertyName("notes")]
    public object Notes { get; set; }
}