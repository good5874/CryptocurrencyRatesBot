namespace CryptocurrencyRatesBot.DAL.DataBase.Tables
{
    public class CreatingSubscription
    {
        public long Id {  get; set; }
        public string Symbol {  get; set; }
        public double? Percent {  get; set; }
        public int? TimeMin {  get; set; }
    }
}
