using Newtonsoft.Json.Linq;
using SteamDB_Crawler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace SteamDB_Crawler
{
    public class SteamDB
    {
        public static List<GameModel> GetFreeGames()
        {
            List<GameModel> gameModels = new List<GameModel>();

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers["User-Agent"] = @"Mozilla / 5.0(Windows NT 10.0; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 81.0.4044.138 Safari / 537.36";
                    string htmlCode = client.DownloadString("https://steamdb.info/upcoming/free/");

                    Regex regAPPID = new Regex("data-appid=\"(.*?)\"");
                    MatchCollection matchesAPPID = regAPPID.Matches(htmlCode);

                    foreach (Match match in matchesAPPID)
                    {
                        string appID = match.Groups[1].Value;

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

                        bool isDLC = steamItemType.ToLower() == "dlc";

                        GameModel model = new GameModel();
                        model.steamLink = url;
                        model.gameBanner = img;
                        model.name = name;
                        model.isDLC = isDLC;

                        gameModels.Add(model);
                    }
                }
            }
            catch { }

            return gameModels;
        }
    }
}
