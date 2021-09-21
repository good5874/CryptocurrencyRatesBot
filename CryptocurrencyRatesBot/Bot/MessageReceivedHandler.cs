using CryptocurrencyRatesBot.DAL.DataBase;
using CryptocurrencyRatesBot.DAL.DataBase.Tables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CryptocurrencyRatesBot.Bot
{
    public partial class Handlers
    {
        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            if (message.Type != MessageType.Text)
                return;

            CreatingSubscription creatingSubscription = null;
            using (var context = new BotDbContext())
            {
                creatingSubscription = context.Users
                                            .Include(x => x.CreatingSubscription)
                                            .FirstOrDefault(x => x.Id == message.From.Id).CreatingSubscription;
            }

            Task<Message> action;
            switch (true)
            {
                case bool _ when Regex.IsMatch(message.Text, "/Start", RegexOptions.IgnoreCase):
                    action = Start(botClient, message);
                    break;

                case bool _ when (Regex.IsMatch(message.Text, @"[A-Z]{3}$")
                                && creatingSubscription.Symbol == null
                                && creatingSubscription.Percent == null
                                && creatingSubscription.TimeMin == null):
                    action = Symbol(botClient, message);
                    break;

                case bool _ when (Regex.IsMatch(message.Text, @"0\,[0-9]{1,6}%$")
                                && creatingSubscription.Symbol != null
                                && creatingSubscription.Percent == null
                                && creatingSubscription.TimeMin == null):
                    action = Percent(botClient, message);
                    break;

                case bool _ when (Regex.IsMatch(message.Text, @"[0-9]{1,4} мин\w*")
                                && creatingSubscription.Symbol != null
                                && creatingSubscription.Percent != null
                                && creatingSubscription.TimeMin == null):
                    action = Time(botClient, message);
                    break;

                case bool _ when Regex.IsMatch(message.Text, @"@\w*"):
                    action = GetUser(botClient, message);
                    break;

                case bool _ when Regex.IsMatch(message.Text, @"/Subscription", RegexOptions.IgnoreCase):
                    action = Subscription(botClient, message);
                    break;

                default:
                    action = Default(botClient, message);
                    break;
            }


            var sentMessage = await action;

            static async Task<Message> Start(ITelegramBotClient botClient, Message message)
            {
                const string text = "Привет, введи тикет криптовалюты или выбери из тop-10 популярных:\n" +
                    "Top-10 популярных:";

                List<Currency> currencies = null;
                using (var context = new BotDbContext())
                {
                    currencies = context.Currencies
                          .OrderByDescending(x => x.CurrencyPriceUSD)
                          .Take(10).ToList();

                    if (context.Users.FirstOrDefault(x => x.Id == message.From.Id) == null)
                    {
                        CustomUser user = new CustomUser()
                        {
                            Id = message.From.Id,
                            FirstName = message.From.FirstName,
                            LastName = message.From.LastName,
                            Username = message.From.Username,
                            CanJoinGroups = message.From.CanJoinGroups,
                            CanReadAllGroupMessages = message.From.CanReadAllGroupMessages,
                            IsBot = message.From.IsBot,
                            LanguageCode = message.From.LanguageCode,
                            SupportsInlineQueries = message.From.SupportsInlineQueries,

                            IsAdmin = false,
                            Status = "Active",
                            CreatingSubscription = new CreatingSubscription()
                        };

                        context.Users.Add(user);
                        await context.SaveChangesAsync();
                    }
                }

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(currencies[0].Symbol, $"select {currencies[0].Symbol}"),
                        InlineKeyboardButton.WithCallbackData(currencies[1].Symbol, $"select {currencies[1].Symbol}"),
                        InlineKeyboardButton.WithCallbackData(currencies[2].Symbol, $"select {currencies[2].Symbol}"),
                        InlineKeyboardButton.WithCallbackData(currencies[3].Symbol, $"select {currencies[3].Symbol}"),
                        InlineKeyboardButton.WithCallbackData(currencies[4].Symbol, $"select {currencies[4].Symbol}"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(currencies[5].Symbol, $"select {currencies[5].Symbol}"),
                        InlineKeyboardButton.WithCallbackData(currencies[6].Symbol, $"select {currencies[6].Symbol}"),
                        InlineKeyboardButton.WithCallbackData(currencies[7].Symbol, $"select {currencies[7].Symbol}"),
                        InlineKeyboardButton.WithCallbackData(currencies[8].Symbol, $"select {currencies[8].Symbol}"),
                        InlineKeyboardButton.WithCallbackData(currencies[9].Symbol, $"select {currencies[9].Symbol}"),
                    },
                });

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: text,
                                                            replyMarkup: inlineKeyboard);
            }

            static async Task<Message> Symbol(ITelegramBotClient botClient, Message message)
            {
                Currency currency = null;
                using (var context = new BotDbContext())
                {
                    currency = context.Currencies.FirstOrDefault(x => message.Text.Contains(x.Symbol));
                    List<Subscription> subscriptions = context.Subscriptions
                        .Include(x => x.Currency)
                        .Where(x => x.User.Id == message.From.Id).ToList();

                    CreatingSubscription creatingSubscription = context.Users
                                            .Include(x => x.CreatingSubscription)
                                            .FirstOrDefault(x => x.Id == message.From.Id).CreatingSubscription;

                    if (subscriptions.FirstOrDefault(x => x.Currency.Symbol == message.Text) != null)
                    {
                        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                                text: $"Такая подписка существует");

                    }
                    if (currency == null || creatingSubscription == null)
                    {
                        return await Default(botClient, message);
                    }

                    creatingSubscription.Symbol = currency.Symbol;
                    context.CreatingSubscriptions.Update(creatingSubscription);
                    await context.SaveChangesAsync();
                }

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                      new []
                      {
                        InlineKeyboardButton.WithCallbackData("0,05%", "select percent 0,05%"),
                        InlineKeyboardButton.WithCallbackData("0,1%", "select percent 0,1%"),
                        InlineKeyboardButton.WithCallbackData("0,15%", "select percent 0,15%"),
                        InlineKeyboardButton.WithCallbackData("0,20%", "select percent 0,20%"),
                        InlineKeyboardButton.WithCallbackData("0,25%", "select percent 0,25%"),
                      }
                });

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                             text: $"Вы выбрали {currency.Symbol}\n" +
                                                                    "Введите процент или выберите ниже:",
                                                             replyMarkup: inlineKeyboard);
            }

            static async Task<Message> Percent(ITelegramBotClient botClient, Message message)
            {
                var isDouble = double.TryParse(message.Text.Replace("%", ""), out double percent);

                CreatingSubscription creatingSubscription;
                using (var context = new BotDbContext())
                {
                    creatingSubscription = context.Users
                        .Include(x => x.CreatingSubscription)
                        .FirstOrDefault(x => x.Id == message.From.Id)
                        .CreatingSubscription;

                    if (!isDouble || creatingSubscription == null)
                    {
                        return await Default(botClient, message);
                    }

                    creatingSubscription.Percent = percent;
                    context.CreatingSubscriptions.Update(creatingSubscription);
                    await context.SaveChangesAsync();
                }

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("5 минут", $"select 5 мин"),
                        InlineKeyboardButton.WithCallbackData("10 минут", $"select 10 мин"),
                        InlineKeyboardButton.WithCallbackData("1 час", $"select 60 мин"),
                        InlineKeyboardButton.WithCallbackData("24 часа", $"select 1440 мин"),
                    }
                });

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                             text: $"Вы выбрали {creatingSubscription.Symbol}\n" +
                                                                   $"Выбранный процент - {creatingSubscription.Percent}\n" +
                                                                    "Введите выбрать временной промежуток для информирования или выберите ниже:",
                                                             replyMarkup: inlineKeyboard);
            }

            static async Task<Message> Time(ITelegramBotClient botClient, Message message)
            {
                var isInt = int.TryParse(message.Text.Split().First(), out int min);

                using (var context = new BotDbContext())
                {

                    CustomUser user = context.Users
                         .Include(x => x.CreatingSubscription)
                         .FirstOrDefault(x => x.Id == message.From.Id);
                    CreatingSubscription creatingSubscription = user.CreatingSubscription;

                    Currency currency = context.Currencies.FirstOrDefault(x => x.Symbol == creatingSubscription.Symbol);

                    if (!isInt || creatingSubscription == null || currency == null)
                    {
                        return await Default(botClient, message);
                    }

                    Subscription subscription = new Subscription
                    {
                        Minutes = min,
                        Percent = creatingSubscription.Percent.Value,
                        LastNotification = DateTime.Now,
                        PriceSentUSD = currency.CurrencyPriceUSD,
                        User = user,
                        Currency = currency,
                    };

                    creatingSubscription.Symbol = null;
                    creatingSubscription.Percent = null;
                    creatingSubscription.TimeMin = null;

                    context.CreatingSubscriptions.Update(creatingSubscription);
                    context.Subscriptions.Add(subscription);
                    await context.SaveChangesAsync();

                    return await botClient
                        .SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: $"Подписка создана\n" +
                                                            $"Криптовалюта - {subscription.Currency.Symbol}\n" +
                                                            $"Процент - {subscription.Percent}\n" +
                                                            $"Текущая цена - {subscription.PriceSentUSD}$\n" +
                                                            $"Временной промежуток для информирования - {subscription.Minutes} мин");
                }
            }

            static async Task<Message> Subscription(ITelegramBotClient botClient, Message message)
            {
                try
                {
                    InlineKeyboardMarkup inlineKeyboardMarkup;
                    using (var context = new BotDbContext())
                    {
                        var subscription = context.Subscriptions
                                                        .Include(x => x.Currency)
                                                        .Where(x => x.UserId == message.From.Id).ToList();
                        inlineKeyboardMarkup = ConvertToTelegramButtons(subscription);
                    }

                    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                                 text: $"Ваши подписки:\n",
                                                                 replyMarkup: inlineKeyboardMarkup);
                }
                catch
                {
                    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                                text: $"У вас нет подписок");
                }
            }

            static async Task<Message> GetUser(ITelegramBotClient botClient, Message message)
            {
                using (var context = new BotDbContext())
                {
                    CustomUser userAdmin = context.Users
                        .Include(x => x.Subscriptions)
                        .FirstOrDefault(x => x.IsAdmin == true && x.Id == message.From.Id);

                    if (userAdmin != null)
                    {
                        CustomUser user = context.Users
                           .Include(x => x.Subscriptions)
                           .FirstOrDefault(x => x.IsAdmin == true && x.Username == message.Text.Replace("@", ""));

                        if (user != null)
                        {
                            string tmp = "";
                            foreach (var sub in user.Subscriptions)
                            {
                                tmp += $"Id подписки - {sub.Id}\nId валюты - {sub.CurrencyId}\n\n";
                            }

                            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                                text: $"Юзернейм - {user.Username}\nПодписки:\n" + tmp);

                        }
                        else
                        {
                            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "Такого юзера не существует");
                        }
                    }
                }
                return await Default(botClient, message);
            }

            static async Task<Message> Default(ITelegramBotClient botClient, Message message)
            {
                using (var context = new BotDbContext())
                {
                    CreatingSubscription creatingSubscription = context.Users
                        .Include(x => x.CreatingSubscription)
                        .FirstOrDefault(x => x.Id == message.From.Id)
                        .CreatingSubscription;

                    if (creatingSubscription != null)
                    {
                        creatingSubscription.Symbol = null;
                        creatingSubscription.Percent = null;
                        creatingSubscription.TimeMin = null;
                        context.CreatingSubscriptions.Update(creatingSubscription);
                        await context.SaveChangesAsync();
                    }
                }

                await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "Команды не существует");
                return await Start(botClient, message);
            }
        }

        public static InlineKeyboardMarkup ConvertToTelegramButtons(List<Subscription> subscriptions)
        {
            var buttons = new List<List<InlineKeyboardButton>>();

            int processesCount = 0;
            while (subscriptions.Count() > processesCount)
            {
                var buttonsLine = subscriptions.Skip(processesCount).Take(5);

                buttons.Add(new List<InlineKeyboardButton>() { });

                foreach (var button in buttonsLine)
                {
                    string buttonData = $"delete {button.Currency.Symbol}";
                    string buttonText = button.Currency.Symbol;

                    buttons.Last().Add(InlineKeyboardButton.WithCallbackData(buttonText, buttonData));
                }

                processesCount += 5;
            }

            return new InlineKeyboardMarkup(buttons);
        }
    }
}
