using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace CryptocurrencyRatesBot.Bot
{
    public class TelegramBot : IDisposable
    {
        private readonly ITelegramBotClient botClient;

        private readonly CancellationTokenSource CancellationTokenSource;

        public TelegramBot()
        {

            CancellationTokenSource = new CancellationTokenSource();
            botClient = new TelegramBotClient(Settings.TokenTelegramBot);

            botClient.StartReceiving(new DefaultUpdateHandler(Handlers.HandleUpdateAsync, Handlers.HandleErrorAsync),
                               CancellationTokenSource.Token);
        }

       

        public void Dispose()
        {
            CancellationTokenSource.Cancel();
        }
    }
}
