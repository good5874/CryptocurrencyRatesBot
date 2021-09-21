using System;

namespace CryptocurrencyRatesBot.DAL.DataBase.Tables
{
    public class Currency
    {
        public long Id { get; set; }
        public string Symbol { get; set; }
        public double CurrencyPriceUSD { get; set; }
        public DateTime Date { get; set; }
    }
}
