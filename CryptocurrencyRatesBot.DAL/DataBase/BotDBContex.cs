using CryptocurrencyRatesBot.DAL.DataBase.Tables;
using Microsoft.EntityFrameworkCore;

namespace CryptocurrencyRatesBot.DAL.DataBase
{
    public class BotDbContext : DbContext
    {
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<CustomUser> Users { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<CreatingSubscription> CreatingSubscriptions { get; set; }

        public BotDbContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Currency>()
                .Property(x => x.Id)
                .ValueGeneratedNever();
            
            modelBuilder.Entity<CustomUser>()
                .Property(x => x.Id)
                .ValueGeneratedNever();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=WIN-AH0B86FQ7GQ\\MSSQLSERVER2019;Initial Catalog=CryptocurrencyRatesBot;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }
    }
}
