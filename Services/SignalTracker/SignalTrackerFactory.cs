using Microsoft.Extensions.Options;
using TradingVault.Interfaces;
using TradingVault.Interfaces.SignalTracker;
using TradingVault.Options;
using TradingVault.Options.TradingOptions;
using TradingVault.Services.SymbolBasedArchitecture;

namespace TradingVault.Services.SignalTracker;

public class SignalTrackerFactory(
    IIndicatorService indicatorService,
    IBinanceClient binanceClient,
    ITelegramService telegramService,
    IKlineProcessor klineProcessor,
    IOptions<TradingOptions> tradingOptions)
    : ISignalTrackerFactory
{
    private readonly List<SignalTracker> _trackers = new();
    private CancellationTokenSource _cts = new();

    public SignalTracker Create(string symbol, string interval, int rsiThreshold) =>
        new(
            symbol,
            interval,
            rsiThreshold,
            indicatorService,
            binanceClient,
            telegramService,
            tradingOptions
        );

    public async Task RequestActiveTrackersCount() =>
        await telegramService.SendMessageAsync(_trackers.Count.ToString());

    public async Task CreateTrackersForUsdcPairsAsync(string interval, int rsiValue)
    {
        var usdcPairs = await klineProcessor.SelectTradingPairsForQuoteAsset();

        foreach (var tracker in usdcPairs.Select(symbol =>
                     new SignalTracker(symbol, interval, rsiValue, indicatorService, binanceClient, telegramService,
                         tradingOptions)))
        {
            _trackers.Add(tracker);
            _ = Task.Run(() => tracker.InitializeBuySignalAsync());
        }

        // start update loop for this interval
        _ = Task.Run(() => UpdateLoopAsync(interval, _cts.Token));
        await telegramService.SendMessageAsync($"Active Trackers:  {_trackers.Count}");
    }

    // private async Task UpdateLoopAsync(string interval, CancellationToken token)
    // {
    //     // Binance interval to milliseconds
    //     var delay = IntervalToDelay(interval);
    //
    //     while (!token.IsCancellationRequested)
    //     {
    //         await UpdateTrackersAsync(interval);
    //         await Task.Delay(delay, token);
    //     }
    // }
    
    private async Task UpdateLoopAsync(string interval, CancellationToken token)
    {
        var intervalSpan = IntervalToDelay(interval);
        var initialDelay = GetInitialDelay(intervalSpan);

        Console.WriteLine($"First update for {interval} in {initialDelay.TotalSeconds:F0}s, then every {intervalSpan}.");

        // Wait for the first candle close
        await Task.Delay(initialDelay, token);

        while (!token.IsCancellationRequested)
        {
            await UpdateTrackersAsync(interval);
            await Task.Delay(intervalSpan, token);
        }
    }

    private async Task UpdateTrackersAsync(string interval)
    {
        var trackersForInterval = _trackers.Where(t => t._interval == interval);

        foreach (var tracker in trackersForInterval) 
            Task.Run(() => tracker.UpdateBuySignalAsync());
    }

    private static TimeSpan IntervalToDelay(string interval) =>
        interval switch
        {
            "1m" => TimeSpan.FromMinutes(1),
            "5m" => TimeSpan.FromMinutes(5),
            "15m" => TimeSpan.FromMinutes(15),
            "1h" => TimeSpan.FromHours(1),
            "4h" => TimeSpan.FromHours(4),
            "1d" => TimeSpan.FromDays(1),
            _ => TimeSpan.FromMinutes(1)
        };
    
    // private static TimeSpan GetInitialDelay(TimeSpan interval)
    // {
    //     var now = DateTime.UtcNow;
    //     var nextAlignedTime = new DateTime(
    //         now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, DateTimeKind.Utc);
    //
    //     // Add interval until it reaches next multiple
    //     while (nextAlignedTime <= now)
    //         nextAlignedTime = nextAlignedTime.Add(interval);
    //
    //     var delay = nextAlignedTime - now;
    //
    //     // Add a small buffer (e.g., 2 seconds) to ensure candle closes on Binance side
    //     return delay + TimeSpan.FromSeconds(5);
    // }
    
    // private static TimeSpan GetInitialDelay(TimeSpan interval)
    // {
    //     var now = DateTime.UtcNow;
    //     var nextAlignedTime = new DateTime(
    //         now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, DateTimeKind.Utc);
    //
    //     while (nextAlignedTime <= now)
    //         nextAlignedTime = nextAlignedTime.Add(interval);
    //
    //     var delay = nextAlignedTime - now;
    //     return delay + TimeSpan.FromSeconds(5); // small buffer so candle fully closes
    // }
    
    private static TimeSpan GetInitialDelay(TimeSpan interval, int bufferMilliseconds = 0)
    {
        // Align to Unix epoch multiples of interval — this matches exchange kline boundaries.
        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var intervalMs = (long)interval.TotalMilliseconds;
        if (intervalMs <= 0) intervalMs = 60_000; // safe fallback to 1m

        var remainder = nowMs % intervalMs;
        var msToNext = remainder == 0 ? intervalMs : intervalMs - remainder;

        // Add a small buffer so the candle is fully finalized on the exchange side
        var totalMs = msToNext + bufferMilliseconds;
        return TimeSpan.FromMilliseconds(totalMs);
    }


    public void StopAll()
    {
        foreach (var signalTracker in _trackers)
            signalTracker.Stop();

        _trackers.Clear();
        telegramService.SendMessageAsync("All trackers stopped");
        foreach (var t in _trackers)
        {
            telegramService.SendMessageAsync($"Active Tracker {t._symbol}");
        }
    }

    public void StopTrackersForInterval(string interval) //TODO complete this one 
    {
        foreach (var tracker in _trackers.Where(tracker => tracker._interval == interval)) tracker.Stop();
    }
}