using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CShark_lab10
{
    internal class Program
    {
        static HttpClient httpClient = new();
        const string file = @"C:\Users\nikpop\Desktop\ticker.txt";
        static string? apiKey; // from console
        public static string? pswd;
        static DateOnly dateFrom = new(2025, 9, 1);
        static DateOnly dateTo = new(2025, 10, 4);

        static async Task Main(string[] args)
        {
            //using (ApplicationContext context = new ApplicationContext()) { }
            //apiKey = args[0];//Console.ReadLine();
            pswd = args[1];//Console.ReadLine();
            //await foreach (var s in ReadTicker(file))
            //{
            //    await SetTicker(httpClient, s);
            //}

            string? userTicker = Console.ReadLine();
            GetTicker(userTicker);
        }

        static void GetTicker(string? userTicker)
        {
            if(userTicker == null) return;
            using ApplicationContext context = new ApplicationContext();
            var Ticker = context.Tickers.Include(p => p.Prices).Include(t => t.TodaysCondition).Where(t => t.Ticker == userTicker).FirstOrDefault();
            //Console.WriteLine(Ticker);
            for(int i = 0; i < Ticker.Prices.Count; i++)
            {
                Console.WriteLine($"{Ticker.Prices[i].Price}\t{Ticker.TodaysCondition[i].State}");
            }
            Console.WriteLine();
        }
        static async Task SetTicker(HttpClient httpClient, string ticker)
        {
            string url = $"https://api.marketdata.app/v1/stocks/candles/D/{ticker}/?from={dateFrom:o}&to={dateTo:o}&token={apiKey}";
            try
            {
                using ApplicationContext context = new ApplicationContext();

                Tickers Ticker = new Tickers { Ticker = ticker };
                await context.AddAsync(Ticker);
                await context.SaveChangesAsync();

                string json = await httpClient.GetStringAsync(url);
                Arr? arr = JsonSerializer.Deserialize<Arr>(json);

                int size = arr.arr.Length;
                List<Prices> prices = new(size);
                List<TodaysCondition> conditions = new(size);
                DateOnly date = dateFrom;

                for(int i = 0; i < size; i++)
                {
                    prices.Add( new Prices { TickerId = Ticker.Id, Price = arr.arr[i], Date = date });

                    string state;
                    if (i-1 > 0 && prices[i].Price > prices[i - 1].Price) { state = "Up"; }
                    else { state = "Down"; }
                    conditions.Add(new TodaysCondition { TickerId = Ticker.Id, State = state });

                    date = date.AddDays(1); // увеличиваем дату на день
                }

                await context.AddRangeAsync(prices);
                await context.AddRangeAsync(conditions);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception on {ticker}\n{ex}\n");
            }

        }
        static async IAsyncEnumerable<string> ReadTicker(string file)
        {
            using StreamReader reader = new(file);
            string? text;
            while ((text = await reader.ReadLineAsync()) != null)
            {
                yield return text;
            }
        }

    }

    public class ApplicationContext : DbContext
    {
        public DbSet<Tickers> Tickers { get; set; }
        public DbSet<Prices> Prices { get; set; }
        public DbSet<TodaysCondition> todaysConditions { get; set; }
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

    public class Tickers
    {
        public int Id { get; set; }
        public string? Ticker { get; set; }
        public List<TodaysCondition> TodaysCondition { get; set; } = new();
        //[JsonPropertyName("c")]
        public List<Prices> Prices { get; set; } = new();
        public Tickers() { }
    }

    public class TodaysCondition
    {
        public int Id { get; set; }
        public int TickerId { get; set; }
        public string? State { get; set; }
        public Tickers? Ticker { get; set; }
        public TodaysCondition() { }
    }
    class Arr // вспомогательный классс для десериализации цен
    {
        [JsonPropertyName("c")]
        public double[]? arr {  get; set; }
    }
    public class Prices
    {
        public int Id { get; set; }
        public int TickerId { get; set; }
        public double? Price { get; set; }
        public DateOnly? Date { get; set; }
        public Tickers Ticker { get; set; }
        public Prices() { }
    }
}

