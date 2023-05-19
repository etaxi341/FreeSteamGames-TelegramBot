using DataManager.Data;
using DataManager.Models;
using SteamCrawler;
using SteamCrawler.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.Enums;
using SteamCrawler.Models.Steam;

namespace FreeSteamGames_TelegramBot;

class Program
{
    static TelegramBotClient bot;
    static User botUser;
    static List<GameModel> games = new();

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

        using CancellationTokenSource cts = new();
        ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };

        bot = new TelegramBotClient(botToken);
        bot.StartReceiving(Bot_HandleUpdateAsync,
                           Bot_HandleErrorAsync,
                           receiverOptions,
                           cts.Token);

        botUser = await bot.GetMeAsync();

        Steam.OnFreeGameReturnedEvent += OnFreeGameReturnedEvent;

        while (true)
        {
            new Thread(() => Steam.Crawl()).Start(); //Run in thread so I don't block the message reading thread

            var timeOfDay = DateTime.Now.TimeOfDay;
            var nextFullHour = TimeSpan.FromHours(Math.Ceiling(timeOfDay.Add(TimeSpan.FromMinutes(-1)).TotalHours)).Add(TimeSpan.FromMinutes(1)); //One Minute after next full hour
            int delta = (int)((nextFullHour - timeOfDay).TotalMilliseconds);

            Thread.Sleep(delta); //Sleep until next hour + 1 minute (High chance to catch new sales early)
        }
    }

    public static Task Bot_HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }

    public static async Task Bot_HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var handler = update.Type switch
        {
            // UpdateType.Unknown: 
            // UpdateType.ChannelPost: 
            // UpdateType.EditedChannelPost: 
            // UpdateType.ShippingQuery: 
            // UpdateType.PreCheckoutQuery: 
            // UpdateType.Poll: 
            UpdateType.Message => Bot_OnMessage(botClient, update.Message!),
            //UpdateType.EditedMessage: 
            UpdateType.CallbackQuery => Bot_OnCallbackQuery(botClient, update.CallbackQuery!),
            //UpdateType.InlineQuery: 
            //UpdateType.ChosenInlineResult: 
            _ => UnknownUpdateHandlerAsync(botClient, update)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await Bot_HandleErrorAsync(botClient, exception, cancellationToken);
        }
    }

    private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        Console.WriteLine($"Unknown update type: {update.Type}");
        return Task.CompletedTask;
    }

    private static Task Bot_OnCallbackQuery(object sender, CallbackQuery callback)
    {
        long chatID = callback.Message.Chat.Id;

        DatabaseContext db = new();
        Subscribers sub = db.subscribers.Where(s => s.chatID == chatID).FirstOrDefault();
        if (sub == null)
        {
            sub = new();
            sub.chatID = chatID;
            sub.wantsDlcInfo = false;
            sub.wantsGameInfo = false;
            db.subscribers.Add(sub);
        }

        switch (callback.Data)
        {
            case gameOnly:
                sub.wantsDlcInfo = false;
                sub.wantsGameInfo = true;

                bot.AnswerCallbackQueryAsync(
                    callbackQueryId: callback.Id,
                    text: "Thank you, I will let you know when I find some free games!"
                );
                break;
            case gameanddlc:
                sub.wantsDlcInfo = true;
                sub.wantsGameInfo = true;

                bot.AnswerCallbackQueryAsync(
                    callbackQueryId: callback.Id,
                    text: "Thank you, I will let you know when I find some free games and dlcs!"
                );
                break;
            case dlcsOnly:
                sub.wantsDlcInfo = true;
                sub.wantsGameInfo = false;

                bot.AnswerCallbackQueryAsync(
                    callbackQueryId: callback.Id,
                    text: "Thank you, I will let you know when I find some free dlcs!"
                );
                break;
        }

        SendFreeGameMessage(sub);
        db.SaveChanges();
        return Task.CompletedTask;
    }

    private static void OnFreeGameReturnedEvent(List<GameModel> gameModels)
    {
        foreach (GameModel gameModel in gameModels)
        {
            Uri uri = new(gameModel.steamLink);
            if (!string.IsNullOrEmpty(uri.Query))
                gameModel.steamLink = uri.AbsoluteUri.Replace(uri.Query, "");
            gameModel.steamLink = gameModel.steamLink.ToLower();
        }

        games = gameModels;

        DatabaseContext db = new();
        Subscribers[] subs = db.subscribers.Where(s => s.wantsDlcInfo || s.wantsGameInfo).ToArray();

        foreach (Subscribers sub in subs)
        {
            Thread messageThread = new(() => SendFreeGameMessage(sub));
            messageThread.Start();

            Thread.Sleep(1000 / 30);
        }
    }

    static void SendFreeGameMessage(Subscribers sub)
    {
        try
        {
            DatabaseContext db = new();
            foreach (var game in games)
            {
                if ((sub.wantsDlcInfo && game.gameType == "dlc") || (sub.wantsGameInfo && game.gameType == "game"))
                {

                    if (db.notifications.Any(n => n.steamLink == game.steamLink && n.chatID == sub.chatID))
                        continue;

                    bot.SendTextMessageAsync(sub.chatID, game.name + " just went free! " + game.steamLink);

                    Notifications notification = new();
                    notification.chatID = sub.chatID;
                    notification.steamLink = game.steamLink;
                    db.notifications.Add(notification);

                    for (int i = 0; i < 5; i++) //Retry saving if fails because other thread is using it.
                    {
                        try
                        {
                            db.SaveChanges();
                            break;
                        }
                        catch
                        {
                            if (i >= 5)
                                throw;
                        }
                        Thread.Sleep(50);
                    }

                    Thread.Sleep(1000); //Sleep 1 second between every message so I don't hit any limits per chat
                }
            }
        }
        catch { }
    }

    private static Task Bot_OnMessage(object sender, Message message)
    {
        long chatID = message.Chat.Id;

        DatabaseContext db = new();
        Subscribers sub = db.subscribers.Where(s => s.chatID == chatID).FirstOrDefault();
        if (sub == null)
        {
            sub = new();
            sub.chatID = chatID;
            sub.wantsDlcInfo = false;
            sub.wantsGameInfo = false;
            db.subscribers.Add(sub);
        }

        string command = message.Text;

        if (!string.IsNullOrEmpty(command))
            command = command.ToLower().Replace("@" + botUser.Username.ToLower(), "");

        switch (command)
        {
            case start:
                InlineKeyboardMarkup replyMarkup = new(
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
        return Task.CompletedTask;
    }
}