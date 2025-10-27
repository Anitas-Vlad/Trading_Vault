using Microsoft.EntityFrameworkCore;
using TradingVault.Models;
using TradingVault.Responses;

namespace TradingVault.Context;

public class TradeVaultContext : DbContext
{
    public TradeVaultContext(DbContextOptions<TradeVaultContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // ModelBuilderExtensions.Seed(modelBuilder);
    }

    public DbSet<PriceResponse> PriceResponses { get; set; } = default!;
    // public DbSet<Coin> Coins { get; set; } = default!;
    public DbSet<BinanceKline> Klines { get; set; } = default!;

    private static class ModelBuilderExtensions
    {
        // public static void Seed(ModelBuilder modelBuilder)
        // {
        //     modelBuilder.Entity<Coin>().HasData(
        //         new Coin { Id = 1, Symbol = "btc" },
        //         new Coin { Id = 2, Symbol = "eth" },
        //         new Coin { Id = 3, Symbol = "bnb" },
        //         new Coin { Id = 4, Symbol = "xrp" },
        //         new Coin { Id = 5, Symbol = "ada" },
        //         new Coin { Id = 6, Symbol = "sol" },
        //         new Coin { Id = 7, Symbol = "dot" },
        //         new Coin { Id = 8, Symbol = "matic" },
        //         new Coin { Id = 9, Symbol = "doge" },
        //         new Coin { Id = 10, Symbol = "ltc" }
        //     );
        // }
    }
}