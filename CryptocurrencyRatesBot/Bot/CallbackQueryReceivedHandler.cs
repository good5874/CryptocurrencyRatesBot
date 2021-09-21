using CryptocurrencyRatesBot.DAL.DataBase;
using CryptocurrencyRatesBot.DAL.DataBase.Tables;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CryptocurrencyRatesBot.Bot
{
    public partial class Handlers
    {
        private static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            Task action;
            switch (true)
            {
                case bool _ when Regex.IsMatch(callbackQuery.Data, "delete [A-Z]{3}$"):
                    action = DeleteCurrecy(botClient, callbackQuery);
                    break;
                
                case bool _ when Regex.IsMatch(callbackQuery.Data, "select [A-Z]{3}$"):
                    callbackQuery.Message.From.Id = callbackQuery.From.Id;
                    callbackQuery.Message.Chat.Id = callbackQuery.From.Id;
                    callbackQuery.Message.Text = callbackQuery.Data.Split()[1];
                    action = Handlers.BotOnMessageReceived(botClient, callbackQuery.Message);
                    break;
                
                case bool _ when Regex.IsMatch(callbackQuery.Data, @"select percent 0\,[0-9]{1,6}%$"):
                    callbackQuery.Message.From.Id = callbackQuery.From.Id;
                    callbackQuery.Message.Chat.Id = callbackQuery.From.Id;
                    callbackQuery.Message.Text = callbackQuery.Data.Split()[2];
                    action = Handlers.BotOnMessageReceived(botClient, callbackQuery.Message);
                    break;
                
                case bool _ when Regex.IsMatch(callbackQuery.Data, @"select [0-9]{1,4} мин$"):
                    callbackQuery.Message.From.Id = callbackQuery.From.Id;
                    callbackQuery.Message.Chat.Id = callbackQuery.From.Id;
                    var tmp = callbackQuery.Data.Split();
                    callbackQuery.Message.Text = $"{tmp[1]} {tmp[2]}";
                    action = Handlers.BotOnMessageReceived(botClient, callbackQuery.Message);
                    break;

                default:
                    action = Default(botClient, callbackQuery);
                    break;
            }

            await action;

            static async Task DeleteCurrecy(ITelegramBotClient botClient, CallbackQuery callbackQuery)
            {
                using (var context = new BotDbContext())
                {
                    var sym = callbackQuery.Data.Split()[1];
                    var subbscription = context.Subscriptions.FirstOrDefault(x => x.Currency.Symbol == sym);
                    context.Subscriptions.Remove(subbscription);

                    await context.SaveChangesAsync();
                }

                await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: "Удалено");
            }
            
            static async Task Default(ITelegramBotClient botClient, CallbackQuery callbackQuery)
            {
                await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: "Ошибка");
            }
        }
    }
}
