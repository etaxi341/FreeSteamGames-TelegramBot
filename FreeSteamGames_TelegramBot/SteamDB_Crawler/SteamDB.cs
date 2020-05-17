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

            using (WebClient client = new WebClient())
            {
                client.Headers["User-Agent"] = @"Mozilla / 5.0(Windows NT 10.0; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 81.0.4044.138 Safari / 537.36";
                string htmlCode = client.DownloadString("https://steamdb.info/upcoming/free/");

                htmlCode = htmlCode.Split("upcoming-promotions")[0];

                string[] splits = htmlCode.Split("app sub");
                string[] freesplits = splits.Where(s => s.Contains("octicon-infinity")).ToArray();

                foreach (string freesplit in freesplits)
                {
                    var regURL = new Regex("href=\"https:\\/\\/store\\.steampowered\\.com\\/app(.*?)\"");
                    var regIMG = new Regex("src=\"https:\\/\\/steamcdn-a\\.akamaihd\\.net\\/steam\\/apps(.*?)\"");
                    var regNAME = new Regex("<b>(.*?)<\\/b>");
                    var matchesURL = regURL.Matches(freesplit);
                    var matchesIMG = regIMG.Matches(freesplit);
                    var matchesNAME = regNAME.Matches(freesplit);

                    if (matchesURL.Count == 0 || matchesIMG.Count == 0)
                        continue;

                    string url = "https://store.steampowered.com/app" + matchesURL[0].Groups[1].Value;
                    string img = "https://steamcdn-a.akamaihd.net/steam/apps" + matchesIMG[0].Groups[1].Value;
                    string name = matchesNAME[0].Groups[1].Value;

                    bool isDLC = name.Contains("<i class=\"muted\">");
                    name = name.Split("<i class=\"muted\">")[0];

                    GameModel model = new GameModel();
                    model.steamLink = url;
                    model.gameBanner = img;
                    model.name = name;
                    model.isDLC = isDLC;

                    gameModels.Add(model);
                }              
            }

            return gameModels;
        }
    }
}
