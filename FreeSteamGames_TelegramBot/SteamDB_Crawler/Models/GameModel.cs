using System;
using System.Collections.Generic;
using System.Text;

namespace SteamDB_Crawler.Models
{
    public class GameModel
    {
        public string steamLink { get; set; }
        public string gameBanner { get; set; }
        public string name { get; set; }
        public string gameType { get; set; }
    }
}
