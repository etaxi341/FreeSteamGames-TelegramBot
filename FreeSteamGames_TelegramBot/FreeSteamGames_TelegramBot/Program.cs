using DataManager.Data;
using DataManager.Models;
using SteamDB_Crawler;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace FreeSteamGames_TelegramBot
{
    class Program
    {
        static TelegramBotClient bot;
        static List<SteamDB_Crawler.Models.GameModel> games = new List<SteamDB_Crawler.Models.GameModel>();

        static void Main(string[] args)
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
            bot.StartReceiving();

            while (true)
            {
                SendFreeGameMessages();
                Thread.Sleep(3600000); //Check every hour
            }
        }

        static void SendFreeGameMessages()
        {
            games = SteamDB.GetFreeGames();

            DatabaseContext db = new DatabaseContext();
            Subscribers[] subs = db.subscribers.Where(s => s.wantsDlcInfo || s.wantsGameInfo).ToArray();

            foreach(Subscribers sub in subs)
            {
                SendFreeGameMessage(sub);
            }
        }

        static void SendFreeGameMessage(Subscribers sub)
        {
            foreach (var game in games)
            {
                if ((sub.wantsDlcInfo && game.isDLC) || (sub.wantsGameInfo && !game.isDLC))
                {
                    DatabaseContext db = new DatabaseContext();
                    if (db.notifications.Any(n => n.steamLink == game.steamLink && n.chatID == sub.chatID))
                        continue;

                    bot.SendTextMessageAsync(sub.chatID, game.name + " just went free! " + game.steamLink);

                    Notifications notification = new Notifications();
                    notification.chatID = sub.chatID;
                    notification.steamLink = game.steamLink;
                    db.notifications.Add(notification);
                    db.SaveChanges();
                }
            }
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

            const string start = "/start";
            const string gameOnly = "Games Only";
            const string gameanddlc = "Games and DLCs";
            const string dlcsOnly = "DLCs Only";


            switch (e.Message.Text)
            {
                case start:
                    ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup(
                        keyboardRow: new[] { new KeyboardButton(gameOnly), new KeyboardButton(gameanddlc), new KeyboardButton(dlcsOnly) },
                        resizeKeyboard: true,
                        oneTimeKeyboard: true
                    );

                    bot.SendTextMessageAsync(
                        chatId: chatID,
                        text: "What messages do you want to receive?",
                        replyMarkup: replyMarkup
                    );
                    break;
                case gameOnly:
                    sub.wantsDlcInfo = false;
                    sub.wantsGameInfo = true;

                    Console.WriteLine("User " + e.Message.Chat.FirstName + " set to " + e.Message.Text);

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

                    Console.WriteLine("User " + e.Message.Chat.FirstName + " set to " + e.Message.Text);

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

                    Console.WriteLine("User " + e.Message.Chat.FirstName + " set to " + e.Message.Text);

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
    }
}