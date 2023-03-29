using SteamCrawler.Models;
using SteamCrawler.Models.Steam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SteamCrawler;

public class Steam
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
}