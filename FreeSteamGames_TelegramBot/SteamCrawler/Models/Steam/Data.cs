using System.Text.Json.Serialization;

namespace SteamCrawler.Models.Steam;

/// <summary>Main class for the infos</summary>
public class Data
{
    [JsonPropertyName("type")]
    public string SteamAppType { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("steam_appid")]
    public int SteamAppid { get; set; }

    [JsonPropertyName("required_age")]
    public int RequiredAge { get; set; }

    [JsonPropertyName("is_free")]
    public bool IsFree { get; set; }

    [JsonPropertyName("controller_support")]
    public string ControllerSupport { get; set; }

    [JsonPropertyName("detailed_description")]
    public string DetailedDescription { get; set; }

    [JsonPropertyName("about_the_game")]
    public string AboutTheGame { get; set; }

    [JsonPropertyName("short_description")]
    public string ShortDescription { get; set; }

    [JsonPropertyName("supported_languages")]
    public string SupportedLanguages { get; set; }

    [JsonPropertyName("header_image")]
    public string HeaderImage { get; set; }

    [JsonPropertyName("website")]
    public string Website { get; set; }

    [JsonPropertyName("pc_requirements")]
    public object PCRequirements { get; set; } // L'ho convertito in object perché da problemi

    [JsonPropertyName("mac_requirements")]
    public object MacRequirements { get; set; } // L'ho convertito in object perché da problemi

    [JsonPropertyName("linux_requirements")]
    public object LinuxRequirements { get; set; } // L'ho convertito in object perché da problemi

    [JsonPropertyName("legal_notice")]
    public string LegalNotice { get; set; }

    [JsonPropertyName("developers")]
    public string[] Developers { get; set; }

    [JsonPropertyName("publishers")]
    public string[] Publishers { get; set; }

    [JsonPropertyName("price_overview")]
    public PriceOverview PriceOverview { get; set; }

    [JsonPropertyName("packages")]
    public int[] Packages { get; set; }

    [JsonPropertyName("package_groups")]
    public PackageGroups[] PackageGroups { get; set; }

    [JsonPropertyName("platforms")]
    public Platforms Platforms { get; set; }

    [JsonPropertyName("metacritic")]
    public Metacritic Metacritic { get; set; }

    [JsonPropertyName("categories")]
    public Category[] Categories { get; set; }

    [JsonPropertyName("genres")]
    public Genre[] Genres { get; set; }

    [JsonPropertyName("screenshots")]
    public Screenshot[] Screenshots { get; set; }

    [JsonPropertyName("movies")]
    public Movie[] Movies { get; set; }

    [JsonPropertyName("recommendations")]
    public Recommendations Recommendations { get; set; }

    [JsonPropertyName("achievements")]
    public Achievements Achievements { get; set; }

    [JsonPropertyName("release_date")]
    public ReleaseDate ReleaseDate { get; set; }

    [JsonPropertyName("support_info")]
    public SupportInfo SupportInfo { get; set; }

    [JsonPropertyName("background")]
    public string Background { get; set; }

    [JsonPropertyName("background_raw")]
    public string BackgroundRaw { get; set; }

    [JsonPropertyName("content_descriptors")]
    public ContentDescriptors ContentDescriptors { get; set; }
}