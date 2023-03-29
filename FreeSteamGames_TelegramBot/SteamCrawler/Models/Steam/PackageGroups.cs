using System.Text.Json.Serialization;

namespace SteamCrawler.Models.Steam;

public class PackageGroups
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("selection_text")]
    public string SelectionText { get; set; }

    [JsonPropertyName("save_text")]
    public string SaveText { get; set; }

    [JsonPropertyName("display_type")]
    public int DisplayType { get; set; }

    [JsonPropertyName("is_recurring_subscription")]
    public string IsRecurringSubscription { get; set; }

    [JsonPropertyName("subs")]
    public Sub[] Subs { get; set; }
}