using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradingVault.Context;
using TradingVault.Interfaces;
using TradingVault.Interfaces.SignalTracker;
using TradingVault.Interfaces.SymbolBasedArchitecture;
using TradingVault.Options;
using TradingVault.Options.TradingOptions;
using TradingVault.Services;
using TradingVault.Services.SignalTracker;
using TradingVault.Services.SymbolBasedArchitecture;

var host = CreateHostBuilder(args).Build();

var tradeVault = host.Services.GetRequiredService<ITradingVault>();

await tradeVault.Run();

return;

static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            config.SetBasePath(Directory.GetCurrentDirectory());
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            config.AddEnvironmentVariables();
        })
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders(); // Remove all default log providers
            logging.AddConsole(); // Only use Console logging
        })
        .ConfigureServices((hostContext, services) =>
        {
            var configuration = hostContext.Configuration;

            services.Configure<BinanceOptions>(configuration.GetSection("Binance"));
            services.Configure<TradingOptions>(configuration.GetSection("Trading"));

            services.AddSingleton<HttpClient>();

            services.AddSingleton<ITradingVault, TradingVault.TradingVault>();
            
            services.AddSingleton<IBinanceClient, BinanceClient>();
            services.AddSingleton<IInputParser, InputParser>();
            services.AddSingleton<IIndicatorService, IndicatorService>();
            services.AddSingleton<IKlineProcessor, KlineProcessor>();
            services.AddSingleton<ISymbolService, SymbolService>();
            services.AddSingleton<ITelegramService, TelegramService>();
            services.AddSingleton<ITradingAnalyzer, TradingAnalyzer>();
            services.AddSingleton<IOutputService, OutputService>();
            services.AddSingleton<ISignalTracker,SignalTracker>();
            services.AddSingleton<ISignalTrackerFactory,SignalTrackerFactory>();
            services.AddSingleton<ISignalTrackingManager, SignalTrackingManager>();
            services.AddSingleton<ISignalTrackerSB, SignalTrackerSB>();
            services.AddSingleton<ISignalTrackerFactorySB, SignalTrackerFactorySB>();

            // services.AddScoped<IBinanceCandleFetcher, BinanceCandleFetcher>();

            var connectionString = configuration.GetConnectionString("trade-vault-context");
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Connection string 'trade-vault-context' not found.");

            services.AddDbContext<TradeVaultContext>(options =>
                options.UseSqlServer(connectionString));
        });
}