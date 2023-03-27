using SteamDB_Crawler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SteamDB_Crawler;

public class Steam // Renamed class, watchout
{
    public delegate void OnFreeGameReturned(List<GameModel> gameModels);
    public static OnFreeGameReturned OnFreeGameReturnedEvent;

    static List<DateTime> previousCalls = new();

    public static void Crawl()
    {
        DateTime minutesAgo5 = DateTime.Now.AddMinutes(-5);
        previousCalls.RemoveAll(e => e < minutesAgo5);

        YourVoidToChangeInSomethingBetter();
    }

    private static async void YourVoidToChangeInSomethingBetter()
    {
        try
        {
            // set "english" for game details in english (such as description), set Currency to "us" for price is USD or "it" to any price in EU1 (more info at my SteamGifts comment: https://www.steamgifts.com/discussion/qtfCX/what-is-mean-eu2#bDM4UkG)
            string GamesLanguage = "italian";
            string GamesCurrency = "it";

            List<GameModel> gameModels = new(); // Your game models
            Capsule Founded = new(); // The json that Steam give us from the search result just contain the game name and logo link (where we can take the game ID)
                                     //  Would be good to have all the game infos in this json. Sadly we need a second call

            // First simple HttpClient API call
            using HttpClient TheClient = new();
            TheClient.Timeout = TimeSpan.FromSeconds(60);
            TheClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("Your_custom_user_agent", "69")); // Any user agent will be ok

            // The magic link
            using HttpResponseMessage TheResp = await TheClient.GetAsync(
                $"https://store.steampowered.com/search/results/?force_infinite=1&maxprice=free&specials=1&l={GamesLanguage}&json=1");

            if (TheResp.IsSuccessStatusCode)
            {
                string StrRes = await TheResp.Content.ReadAsStringAsync();

                Founded = JsonSerializer.Deserialize<Capsule>(StrRes)!; // Deserialize json in a C# class
            }

            // If Founded and or Games are null, let's thrown an ArgumentNullException. (or you can handle it it the way you want)
            if (Founded is null || Founded.Games is null) { throw new ArgumentNullException(nameof(Founded)); }


            // Founded game foreach
            foreach (Capsule.Game Game in Founded.Games)
            {
                // The link to the game logo that contain the game ID
                string LogoLink = Game.LogoLink;

                // Let's take the game ID from the logo link
                string ID = string.IsNullOrWhiteSpace(LogoLink)
                                    ? throw new ArgumentNullException(nameof(LogoLink))
                                    : Regex.Match(LogoLink, "(?s)http.*?apps\\/([\\s\\S]*?)\\/",
                                    RegexOptions.None, TimeSpan.FromSeconds(5)).Groups[1].Value;

                // Here fill this for now, so in case the second HttpClient will go wrong, we have at least the mandatory infos
                string Title = Game.Name![..300];
                string Link = $"https://store.steampowered.com/app/{ID}/";
                string LinkImg = $"https://cdn.akamai.steamstatic.com/steam/apps/{ID}/header.jpg";

                string Desc = string.Empty;       // Will fill later
                string Price = string.Empty;     // Will fill later
                string GameType = string.Empty; // Will fill later

                // The second call for game details, inside a Try so there is no pain
                try
                {
                    using HttpClient TheDetailClient = new();
                    TheDetailClient.Timeout = TimeSpan.FromSeconds(60);
                    TheDetailClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("Your_custom_user_agent", "69"));

                    using HttpResponseMessage TheDetailResp = await TheDetailClient.GetAsync(
                        $"https://store.steampowered.com/api/appdetails?appids={ID}&l={GamesLanguage}&cc={GamesCurrency}");

                    if (TheDetailResp.IsSuccessStatusCode)
                    {
                        string StrResp2 = await TheDetailResp.Content.ReadAsStringAsync();

                        // Deserializa using .Net 6 build-it classes
                        // Considering Steam return a json with the first property as the game ID, let's skip it and take the sub items
                        var TempDic = JsonSerializer.Deserialize<Dictionary<string, Object>>(StrResp2);
                        string SubJson = TempDic is null ? string.Empty : TempDic.FirstOrDefault().Value.ToString() ?? string.Empty;
                        GameDetails D = string.IsNullOrWhiteSpace(SubJson) ? new() : JsonSerializer.Deserialize<GameDetails>(SubJson) ?? new();

                        // Here are the details. I have added "Price" but you can add whatever you want. At the bottom of the page i have inclued a complete class with all infos
                        if (D is not null && D.Success && D.Data is not null)
                        {
                            // Things you need
                            Desc = string.IsNullOrWhiteSpace(D.Data.ShortDescription) ? "No desc" : D.Data.ShortDescription[..300];
                            LinkImg = string.IsNullOrWhiteSpace(D.Data.HeaderImage) ? LinkImg : D.Data.HeaderImage;
                            GameType = string.IsNullOrWhiteSpace(D.Data.SteamAppType) ? "game" : D.Data.SteamAppType.ToLower();

                            // Extras...
                            Price = D.Data.PriceOverview is null || string.IsNullOrWhiteSpace(D.Data.PriceOverview.InitialFormatted) ? "Idk" : D.Data.PriceOverview.InitialFormatted;
                        }
                    }
                }
                catch { } // Just ignore any error

                // Back to your code again
                GameModel model = new()
                {
                    steamLink = Link,
                    gameBanner = LinkImg,
                    name = Title,
                    gameType = GameType
                };

                if (!gameModels.Any(m => m.steamLink == model.steamLink))
                    gameModels.Add(model);
            }


            if (gameModels.Count > 0)
                OnFreeGameReturnedEvent?.Invoke(gameModels);
        }
        catch { }
    }


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

    // Not used because Steam return different thing based on the game if is avaible for Windows, Mac and Linux
    public class HardwareRequirements
    {
        [JsonPropertyName("minimum")]
        public string Minimum { get; set; }

        [JsonPropertyName("recommended")]
        public string Recommended { get; set; }
    }

    public class PriceOverview
    {
        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("initial")]
        public int Initial { get; set; }

        [JsonPropertyName("final")]
        public int Final { get; set; }

        [JsonPropertyName("discount_percent")]
        public int Discountpercent { get; set; }

        [JsonPropertyName("initial_formatted")]
        public string InitialFormatted { get; set; }

        [JsonPropertyName("final_formatted")]
        public string FinalFormatted { get; set; }
    }

    public class Platforms
    {
        [JsonPropertyName("windows")]
        public bool Windows { get; set; }

        [JsonPropertyName("mac")]
        public bool Mac { get; set; }

        [JsonPropertyName("linux")]
        public bool Linux { get; set; }
    }

    public class Metacritic
    {
        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("url")]
        public string URL { get; set; }
    }

    public class Recommendations
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class Achievements
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("highlighted")]
        public Highlighted[] Highlighted { get; set; }
    }

    public class Highlighted
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }
    }

    public class ReleaseDate
    {
        [JsonPropertyName("coming_soon")]
        public bool ComingSoon { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }
    }

    public class SupportInfo
    {
        [JsonPropertyName("url")]
        public string URL { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }
    }

    public class ContentDescriptors
    {
        [JsonPropertyName("ids")]
        public object[] IDs { get; set; }

        [JsonPropertyName("notes")]
        public object Notes { get; set; }
    }

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

    public class Sub
    {
        [JsonPropertyName("packageid")]
        public int PackageID { get; set; }

        [JsonPropertyName("percent_savings_text")]
        public string PercentSavingsText { get; set; }

        [JsonPropertyName("percent_savings")]
        public int PercentSavings { get; set; }

        [JsonPropertyName("option_text")]
        public string OptionText { get; set; }

        [JsonPropertyName("option_description")]
        public string OptionDescription { get; set; }

        [JsonPropertyName("can_get_free_license")]
        public string CanGetFreeLicense { get; set; }

        [JsonPropertyName("is_free_license")]
        public bool IsFreeLicense { get; set; }

        [JsonPropertyName("price_in_cents_with_discount")]
        public int PriceInCentsWithDiscount { get; set; }
    }

    public class Category
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    public class Genre
    {
        [JsonPropertyName("id")]
        public string ID { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    public class Screenshot
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("path_thumbnail")]
        public string PathThumbnail { get; set; }

        [JsonPropertyName("path_full")]
        public string PathFull { get; set; }
    }

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

    public class Webm
    {
        [JsonPropertyName("480")]
        public string Px480 { get; set; }

        [JsonPropertyName("max")]
        public string Max { get; set; }
    }

    public class Mp4
    {
        [JsonPropertyName("480")]
        public string Px480 { get; set; }

        [JsonPropertyName("max")]
        public string Max { get; set; }
    }
}
