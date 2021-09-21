using Newtonsoft.Json;
using System.Collections.Generic;

namespace CryptocurrencyRatesBot.Web.Models
{
    public class Data
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("values")]
        public Dictionary<string, Dictionary<string, double>> Values { get; set; }
    }
}
