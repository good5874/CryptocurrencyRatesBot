using System;

namespace CryptocurrencyRatesBot.DAL.DataBase.Tables
{
    public class Subscription
    {
        public long Id {  get; set; }   
        public int Minutes {  get; set; }
        public double Percent {  get; set; }
        public DateTime LastNotification {  get; set; }
        public double PriceSentUSD { get; set; }


        public long UserId {  get; set; } 
        public CustomUser User {  get; set; } 
        public long CurrencyId {  get; set; } 
        public Currency Currency {  get; set; } 
    }
}
