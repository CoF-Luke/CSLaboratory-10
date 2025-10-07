using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CShark_lab10
{
    internal class ApplicationContext : DbContext
    {
        public DbSet<Tickers> Tickers { get; set; }
        public DbSet<Prices> Prices { get; set; }
        public DbSet<TodaysCondition> TodaysConditions { get; set; }
        public ApplicationContext()
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql($"Host=localhost;Port=5433;Database=tickersdb;Username=postgres;Password={Program.pswd}");
        }
    }

    internal class Tickers
    {
        public int Id { get; set; }
        public string? Ticker { get; set; }
        public List<TodaysCondition> TodaysCondition { get; set; } = new();
        //[JsonPropertyName("c")]
        public List<Prices> Prices { get; set; } = new();
    }

    internal class TodaysCondition
    {
        public int Id { get; set; }
        public int TickerId { get; set; }
        public string? State { get; set; }
        public Tickers? Ticker { get; set; }
    }
    internal class PricesDeserializer // вспомогательный классс для десериализации цен
    {
        [JsonPropertyName("c")]
        public double[]? Prices { get; set; }
    }
    internal class Prices
    {
        public int Id { get; set; }
        public int TickerId { get; set; }
        public double? Price { get; set; }
        public DateOnly? Date { get; set; }
        public Tickers? Ticker { get; set; }
    }
}
