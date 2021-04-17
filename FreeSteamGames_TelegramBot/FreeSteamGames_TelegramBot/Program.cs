using DataManager.Data;
using DataManager.Models;
using SteamDB_Crawler;
using SteamDB_Crawler.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FreeSteamGames_TelegramBot
{
    class Program
    {
        static TelegramBotClient bot;
        static User botUser;
        static List<SteamDB_Crawler.Models.GameModel> games = new List<SteamDB_Crawler.Models.GameModel>();

        #region COMMANDS
        const string start = "/start";
        const string gameOnly = "Games Only";
        const string gameanddlc = "Games and DLCs";
        const string dlcsOnly = "DLCs Only";
        const string unsubscribe = "/unsubscribe";
        #endregion

        static async Task Main(string[] args)
        {
            string botToken;

            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Please enter your Bot Token. Alternatively you can also use it as startup parameter");
                botToken = Console.ReadLine();
            }
            else
            {
                botToken = args[0];
            }

            bot = new TelegramBotClient(botToken);
            bot.OnMessage += Bot_OnMessage;
            bot.OnCallbackQuery += Bot_OnCallbackQuery;
            bot.StartReceiving();

            botUser = await bot.GetMeAsync();

            SteamDB.OnFreeGameReturnedEvent += OnFreeGameReturnedEvent;

            while (true)
            {
                SteamDB.Crawl();

                var timeOfDay = DateTime.Now.TimeOfDay;
                var nextFullHour = TimeSpan.FromHours(Math.Ceiling(timeOfDay.Add(TimeSpan.FromMinutes(-1)).TotalHours)).Add(TimeSpan.FromMinutes(1)); //One Minute after next full hour
                int delta = (int)((nextFullHour - timeOfDay).TotalMilliseconds);

                Thread.Sleep(delta); //Sleep until next hour + 1 minute (High chance to catch new sales early)
            }
        }

        private static void Bot_OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            long chatID = e.CallbackQuery.Message.Chat.Id;

            DatabaseContext db = new DatabaseContext();
            Subscribers sub = db.subscribers.Where(s => s.chatID == chatID).FirstOrDefault();
            if (sub == null)
            {
                sub = new Subscribers();
                sub.chatID = chatID;
                sub.wantsDlcInfo = false;
                sub.wantsGameInfo = false;
                db.subscribers.Add(sub);
            }

            switch (e.CallbackQuery.Data)
            {
                case gameOnly:
                    sub.wantsDlcInfo = false;
                    sub.wantsGameInfo = true;

                    bot.SendTextMessageAsync(
                        chatId: chatID,
                        text: "Thank you, I will let you know when I find some free games!",
                        replyMarkup: new ReplyKeyboardRemove()
                    );

                    SendFreeGameMessage(sub);
                    break;
                case gameanddlc:
                    sub.wantsDlcInfo = true;
                    sub.wantsGameInfo = true;

                    bot.SendTextMessageAsync(
                        chatId: chatID,
                        text: "Thank you, I will let you know when I find some free games and dlcs!",
                        replyMarkup: new ReplyKeyboardRemove()
                    );

                    SendFreeGameMessage(sub);
                    break;
                case dlcsOnly:
                    sub.wantsDlcInfo = true;
                    sub.wantsGameInfo = false;

                    bot.SendTextMessageAsync(
                        chatId: chatID,
                        text: "Thank you, I will let you know when I find some free dlcs!",
                        replyMarkup: new ReplyKeyboardRemove()
                    );

                    SendFreeGameMessage(sub);
                    break;
            }

            db.SaveChanges();
        }

        private static void OnFreeGameReturnedEvent(List<GameModel> gameModels)
        {
            games = gameModels;

            DatabaseContext db = new DatabaseContext();
            Subscribers[] subs = db.subscribers.Where(s => s.wantsDlcInfo || s.wantsGameInfo).ToArray();

            foreach (Subscribers sub in subs)
            {
                Thread messageThread = new Thread(() => SendFreeGameMessage(sub));
                messageThread.Start();

                Thread.Sleep(20);
            }
        }

        static void SendFreeGameMessage(Subscribers sub)
        {
            try
            {
                DatabaseContext db = new DatabaseContext();
                foreach (var game in games)
                {
                    if ((sub.wantsDlcInfo && game.gameType == "dlc") || (sub.wantsGameInfo && game.gameType == "game"))
                    {
                        
                        if (db.notifications.Any(n => n.steamLink.StartsWith(game.steamLink) && n.chatID == sub.chatID))
                            continue;

                        bot.SendTextMessageAsync(sub.chatID, game.name + " just went free! " + game.steamLink);

                        Notifications notification = new Notifications();
                        notification.chatID = sub.chatID;
                        notification.steamLink = game.steamLink;
                        db.notifications.Add(notification);
                        db.SaveChanges();

                        Thread.Sleep(1000); //Sleep 1 second between every message so I don't hit any limits
                    }
                }
            }
            catch { }
        }

        private static void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            long chatID = e.Message.Chat.Id;

            DatabaseContext db = new DatabaseContext();
            Subscribers sub = db.subscribers.Where(s => s.chatID == chatID).FirstOrDefault();
            if (sub == null)
            {
                sub = new Subscribers();
                sub.chatID = chatID;
                sub.wantsDlcInfo = false;
                sub.wantsGameInfo = false;
                db.subscribers.Add(sub);
            }

            string command = e.Message.Text;

            if (!string.IsNullOrEmpty(command))
                command = command.ToLower().Replace("@" + botUser.Username.ToLower(), "");

            switch (command)
            {
                case start:
                    InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(
                        new[] { InlineKeyboardButton.WithCallbackData(gameOnly), InlineKeyboardButton.WithCallbackData(gameanddlc), InlineKeyboardButton.WithCallbackData(dlcsOnly) }
                    );

                    bot.SendTextMessageAsync(
                        chatId: chatID,
                        text: "What messages do you want to receive?",
                        replyMarkup: replyMarkup
                    );
                    break;
                case unsubscribe:
                    sub.wantsDlcInfo = false;
                    sub.wantsGameInfo = false;

                    bot.SendTextMessageAsync(
                        chatId: chatID,
                        text: "You have unsubscribed, you will be missed.",
                        replyMarkup: new ReplyKeyboardRemove()
                    );
                    break;
            }

            db.SaveChanges();
        }
    }
}