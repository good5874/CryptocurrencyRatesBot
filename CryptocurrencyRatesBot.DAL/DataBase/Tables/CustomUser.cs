using System.Collections.Generic;
using Telegram.Bot.Types;

namespace CryptocurrencyRatesBot.DAL.DataBase.Tables
{
    public class CustomUser: User
    {
        public string Status { get; set; }
        public bool IsAdmin { get; set; }

        public long CreatingSubscriptionId {  get; set; }
        public CreatingSubscription CreatingSubscription {  get; set; }

        public virtual IEnumerable<Subscription> Subscriptions {  get; set; } 
    }
}
