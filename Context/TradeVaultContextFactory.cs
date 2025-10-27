using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TradingVault.Context;

public class TradeVaultContextFactory : IDesignTimeDbContextFactory<TradeVaultContext>
{
    public TradeVaultContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("trade-vault-context");

        var optionsBuilder = new DbContextOptionsBuilder<TradeVaultContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new TradeVaultContext(optionsBuilder.Options);
    }
}