using CryptocurrencyRatesBot.DAL.DataBase;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using CryptocurrencyRatesBot.Bot;
using Quartz;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptocurrencyRatesBot.Web.Services
{
    public class NotificationJob: IJob
    {
        private ITelegramBotClient botClient;

        public NotificationJob()
        {
            botClient = new TelegramBotClient(Settings.TokenTelegramBot);
        }

        public async Task Execute(IJobExecutionContext context)
        {

            using (var contextDB = new BotDbContext())
            {
                var subscriptions = contextDB.Subscriptions
               .Include(x => x.Currency)
               .Include(x => x.User)
               .Where(x => (x.Currency.CurrencyPriceUSD * (1+x.Percent)) > x.PriceSentUSD || 
                           (x.Currency.CurrencyPriceUSD * (1 - x.Percent)) < x.PriceSentUSD).ToList();

                foreach (var subscription in subscriptions)
                {
                    var date = subscription.LastNotification.AddMinutes(subscription.Minutes);
                    if (date <= DateTime.Now)
                    {
                        subscription.LastNotification = DateTime.Now;
                        subscription.PriceSentUSD = subscription.Currency.CurrencyPriceUSD;
                        contextDB.Subscriptions.Update(subscription);
                        await contextDB.SaveChangesAsync();

                        await SendNotification(subscription.User.Id, subscription.Currency.Symbol,
                                        subscription.Percent, subscription.Currency.CurrencyPriceUSD);
                    }
                }
            }
        }

        public async Task<Message> SendNotification(long chatId, string symbol,
           double percent, double currentPriceUSD)
        {
            return await botClient
                        .SendTextMessageAsync(chatId: chatId,
                                                      text: $"Криптовалюта - {symbol}\n" +
                                                            $"Изменение на - {percent}%\n" +
                                                            $"Текущая цена - {currentPriceUSD}$\n");
        }
    }
}
