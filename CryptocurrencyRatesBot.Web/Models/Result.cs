using Newtonsoft.Json;

namespace CryptocurrencyRatesBot.Web.Models
{
    public class Result
    {
        [JsonProperty("data")]
        public Data[] Data { get; set; }
    }
}
