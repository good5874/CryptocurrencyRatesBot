using CryptocurrencyRatesBot.DAL.DataBase;
using CryptocurrencyRatesBot.DAL.DataBase.Tables;
using CryptocurrencyRatesBot.Web.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CryptocurrencyRatesBot.Web.Services
{
    public class CryptorankJob : IJob
    {
        private HttpClient _httpclient;
        private BotDbContext _botDBContex;

        private const string ApiUrl = "https://api.cryptorank.io/v1";
        private const string ApiKey = "0b7fcdebab2abadbf9968d4f22937130022225bec305ec09303792cea249";

        public CryptorankJob()
        {
            _httpclient = new HttpClient();
            _botDBContex = new BotDbContext();
            Initialization().Wait();
        }

        private Result ListCurrencies()
        {
            try
            {
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get,
                    $"{ApiUrl}/currencies?api_key={ApiKey}");

                HttpResponseMessage response = _httpclient.SendAsync(requestMessage).Result;

                string apiResponse = response.Content.ReadAsStringAsync().Result;

                if (apiResponse != "")
                {
                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject<Result>(apiResponse);

                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private async Task Initialization()
        {
            if (_botDBContex.Currencies.Count() < 100)
            {
                var result = ListCurrencies();
                List<Currency> currencies = new List<Currency>();

                ConvertRequestResultToListCurrency(result, currencies);

                await _botDBContex.Currencies.AddRangeAsync(currencies);
                await _botDBContex.SaveChangesAsync();
            }
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var result = ListCurrencies();
            List<Currency> currencies = new List<Currency>();

            ConvertRequestResultToListCurrency(result, currencies);

            _botDBContex.Currencies.UpdateRange(currencies);
            await _botDBContex.SaveChangesAsync();
        }

        private List<Currency> ConvertRequestResultToListCurrency(Result requestResult, List<Currency> currencies)
        {
            foreach (var cur in requestResult.Data)
            {
                currencies.Add(new Currency()
                {
                    Id = cur.Id,
                    Symbol = cur.Symbol,
                    CurrencyPriceUSD = cur.Values.GetValueOrDefault("USD").GetValueOrDefault("price"),
                    Date = DateTime.Now
                });
            }

            return currencies;
        }
    }
}
