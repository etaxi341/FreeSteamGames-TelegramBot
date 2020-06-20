using CefSharp;
using CefSharp.OffScreen;
using Newtonsoft.Json.Linq;
using SteamDB_Crawler.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SteamDB_Crawler
{
    public class SteamDB
    {
        public delegate void OnFreeGameReturned(List<GameModel> gameModels);
        public static OnFreeGameReturned OnFreeGameReturnedEvent;

        static List<DateTime> previousCalls = new List<DateTime>();

        public static void Crawl()
        {
            DateTime minutesAgo5 = DateTime.Now.AddMinutes(-5);
            previousCalls.RemoveAll(e => e < minutesAgo5);

            Browser.OnSourceCodeLoadedEvent -= OnSourceCodeLoadedEvent;
            Browser.OpenUrl("https://steamdb.info/upcoming/free/");
            Browser.OnSourceCodeLoadedEvent += OnSourceCodeLoadedEvent;
        }

        private static void OnSourceCodeLoadedEvent(string src)
        {
            try
            {
                List<GameModel> gameModels = new List<GameModel>();

                Regex regAPPID = new Regex("data-appid=\"(.*?)\"");
                MatchCollection matchesAPPID = regAPPID.Matches(src);

                foreach (Match match in matchesAPPID)
                {
                    DateTime minutesAgo5 = DateTime.Now.AddMinutes(-5);
                    while (previousCalls.Where(e => e > minutesAgo5).Count() >= 190)
                        Thread.Sleep(1000);

                    previousCalls.Add(DateTime.Now);
                    string appID = match.Groups[1].Value;

                    using (WebClient client = new WebClient())
                    {
                        client.Headers["User-Agent"] = @"Mozilla / 5.0(Windows NT 10.0; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 81.0.4044.138 Safari / 537.36";
                        string steamApiJSONString = client.DownloadString("https://store.steampowered.com/api/appdetails/?appids=" + appID + "&cc=EE&l=english&v=1");
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

                        GameModel model = new GameModel();
                        model.steamLink = url;
                        model.gameBanner = img;
                        model.name = name;
                        model.gameType = gameType;

                        gameModels.Add(model);
                    }
                }

                if (gameModels.Count > 0)
                    OnFreeGameReturnedEvent?.Invoke(gameModels);
            }
            catch { }
        }
    }
}
