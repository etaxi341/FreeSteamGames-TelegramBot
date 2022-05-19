using Newtonsoft.Json.Linq;
using SteamDB_Crawler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;

namespace SteamDB_Crawler;

public class SteamDB
{
    public delegate void OnFreeGameReturned(List<GameModel> gameModels);
    public static OnFreeGameReturned OnFreeGameReturnedEvent;

    static List<DateTime> previousCalls = new();

    public static void Crawl()
    {
        DateTime minutesAgo5 = DateTime.Now.AddMinutes(-5);
        previousCalls.RemoveAll(e => e < minutesAgo5);

        Browser.OnSourceCodeLoadedEvent -= OnSourceCodeLoadedEvent;
        Browser.OpenUrl("https://steamdb.info/upcoming/free/");
        Browser.OnSourceCodeLoadedEvent += OnSourceCodeLoadedEvent;
    }

    private static async void OnSourceCodeLoadedEvent(string src)
    {
        try
        {
            List<GameModel> gameModels = new();

            Regex regAPPID = new("data-appid=\"(.*?)\"");
            MatchCollection matchesAPPID = regAPPID.Matches(src);

            foreach (Match match in matchesAPPID)
            {
                DateTime minutesAgo5 = DateTime.Now.AddMinutes(-5);
                while (previousCalls.Where(e => e > minutesAgo5).Count() >= 190)
                {
                    Thread.Sleep(1000);
                    minutesAgo5 = DateTime.Now.AddMinutes(-5);
                }

                previousCalls.Add(DateTime.Now);
                string appID = match.Groups[1].Value;

                using var client = new HttpClient();
                client.DefaultRequestHeaders
                  .UserAgent
                  .TryParseAdd(@"Mozilla / 5.0(Windows NT 10.0; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 81.0.4044.138 Safari / 537.36");

                using HttpResponseMessage response = await client.GetAsync("https://store.steampowered.com/api/appdetails/?appids=" + appID + "&cc=EE&l=english&v=1");
                using HttpContent content = response.Content;
                string steamApiJSONString = await content.ReadAsStringAsync();

                var steamApiJSON = JObject.Parse(steamApiJSONString);

                bool success = steamApiJSON[appID]["success"].ToObject<bool>();
                if (!success)
                    continue;

                bool isFree = steamApiJSON[appID]["data"]["is_free"].ToObject<bool>();

                bool isReleased = !steamApiJSON[appID]["data"]["release_date"]["coming_soon"].ToObject<bool>();

                if (!isFree || !isReleased)
                    continue;

                string url = "https://store.steampowered.com/app/" + appID;
                string img = steamApiJSON[appID]["data"]["header_image"].ToString();
                string name = steamApiJSON[appID]["data"]["name"].ToString();
                string steamItemType = steamApiJSON[appID]["data"]["type"].ToString();

                string gameType = steamItemType.ToLower();

                GameModel model = new();
                model.steamLink = url;
                model.gameBanner = img;
                model.name = name;
                model.gameType = gameType;

                if (!gameModels.Any(m => m.steamLink == model.steamLink))
                    gameModels.Add(model);
            }

            if (gameModels.Count > 0)
                OnFreeGameReturnedEvent?.Invoke(gameModels);
        }
        catch { }
    }
}