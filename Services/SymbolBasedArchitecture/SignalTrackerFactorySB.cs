using Microsoft.Extensions.Options;
using TradingVault.Interfaces;
using TradingVault.Interfaces.SymbolBasedArchitecture;
using TradingVault.Options;
using TradingVault.Options.TradingOptions;

namespace TradingVault.Services.SymbolBasedArchitecture;

public class SignalTrackerFactorySB(
    IIndicatorService indicatorService,
    IBinanceClient binanceClient,
    ITelegramService telegramService,
    IKlineProcessor klineProcessor,
    IOptions<TradingOptions> tradingOptions)
    : ISignalTrackerFactorySB
{
    // private CancellationTokenSource _cts = new();
    private Dictionary<string, CancellationTokenSource> _ctssByInterval = new();
    private List<Symbol> _symbols = [];

    public SignalTrackerSB CreateTracker(string symbol, string interval, int rsiThreshold) =>
        new(
            symbol,
            interval,
            rsiThreshold,
            indicatorService,
            binanceClient,
            telegramService,
            tradingOptions
        );

    public async Task RequestActiveTrackersCount()
    {
        var activeTrackersCount = (_symbols?.Sum(s => s.trackers?.Count ?? 0) ?? 0).ToString();
        await telegramService.SendMessageAsync($"Active Trackers: {activeTrackersCount}");
    }

    public async Task CreateTrackersForUsdcPairsAsync(string interval, int rsiValue)
    {
        var usdcPairs = await klineProcessor.SelectTradingPairsForQuoteAsset();

        if (!_ctssByInterval.ContainsKey(interval))
            _ctssByInterval[interval] = new CancellationTokenSource();
        else
        {
            await telegramService.SendMessageAsync($"⚠️ Trackers for [{interval}] already exist.");
            return;
        }


        foreach (var symbolValue in usdcPairs)
        {
            var symbol = new Symbol(symbolValue);

            var tracker = CreateTracker(symbolValue, interval, rsiValue);
            symbol.trackers.Add(tracker);
            _symbols.Add(symbol);

            // await tracker.SeedAsync();
            _ = Task.Run(() => tracker.SeedAsync()); //TODO Edited this hoping it fixes the initial cold delay issue
        }

        _ = Task.Run(() => UpdateLoopAsync(interval, _ctssByInterval[interval].Token));

        var activeTrackersCount = (_symbols?.Sum(s => s.trackers?.Count ?? 0) ?? 0).ToString();
        await telegramService.SendMessageAsync($"✅ Active Trackers: {activeTrackersCount} for [{interval}]");
    }

    private async Task UpdateLoopAsync(string interval, CancellationToken token)
    {
        var intervalSpan = IntervalToDelay(interval);
        var initialDelay = GetInitialDelay(intervalSpan);

        var nextUpdateTime = DateTime.UtcNow + initialDelay;

        await UpdateTrackersAsync(interval);

        await telegramService.SendMessageAsync(
            $"[{interval}] ⏱️ Tracker started.\nNext update at *{nextUpdateTime:HH:mm:ss.} UTC* " +
            $"(in {initialDelay.TotalSeconds:F0}s)"
        );

        // Wait for first candle close
        await Task.Delay(initialDelay, token);

        while (!token.IsCancellationRequested)
        {
            var loopStart = DateTime.UtcNow;
            // Console.WriteLine($"\n{header} 🔄 Update triggered at {loopStart:HH:mm:ss} UTC");

            // await telegramService.SendMessageAsync($"{header} 🔄 Update triggered at *{loopStartLocal:HH:mm:ss} UTC*"); 
            await telegramService.SendMessageAsync($"{interval} 🔄 Update triggered");

            await UpdateTrackersAsync(interval);

            var nextAlignedTime = loopStart + intervalSpan;
            var nextAlignedTimeLocal = DateTime.Now + intervalSpan;
            await telegramService.SendMessageAsync(
                $"[{interval}] ✅ Done updating. Next at *{nextAlignedTimeLocal:HH:mm:ss} UTC*");

            // Heartbeat countdown (every 5 seconds)
            for (var i = (int)intervalSpan.TotalSeconds; i > 0 && !token.IsCancellationRequested; i -= 5)
            {
                Console.Write($"\r[{interval}] ⏳ Next update in {i}s...");
                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
        }
    }

    private static TimeSpan GetInitialDelay(TimeSpan interval, int bufferMilliseconds = 0)
    {
        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var intervalMs = (long)interval.TotalMilliseconds;
        if (intervalMs <= 0) intervalMs = 60_000; // safe fallback to 1m

        var remainder = nowMs % intervalMs;
        var msToNext = remainder == 0 ? intervalMs : intervalMs - remainder;

        // Add a small buffer so the candle is fully finalized on the exchange side
        var totalMs = msToNext + bufferMilliseconds;
        return TimeSpan.FromMilliseconds(totalMs);
    }

    private async Task UpdateTrackersAsync(string interval)
    {
        var trackersToUpdate = _symbols
            .SelectMany(s => s.trackers)
            .Where(t => t._interval == interval)
            .ToList();

        var tasks = trackersToUpdate.Select(t => t.UpdateBuySignalAsync());
        await Task.WhenAll(tasks);
    }

    public void StopAllTrackers()
    {
        foreach (var symbol in _symbols)
            symbol.StopTrackers();

        foreach (var cts in _ctssByInterval)
        {
            cts.Value.Cancel();
            cts.Value.Dispose();
        }

        _ctssByInterval = new Dictionary<string, CancellationTokenSource>();

        telegramService.SendMessageAsync("All trackers stopped");
    }

    public async Task StopTrackersForInterval(string interval)
    {
        foreach (var symbol in _symbols) 
            symbol.StopTrackersByInterval(interval);

        _ctssByInterval.Remove(interval);

        await telegramService.SendMessageAsync($"Trackers for [{interval}] stopped.");
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

    private bool TrackersByIntervalExist(string interval) =>
        _symbols
            .SelectMany(s => s.trackers)
            .Any(t => t._interval == interval);

    // private bool TrackersAlreadyExistForSymbolAndInterval(string symbolValue, string interval) =>
    //     _symbols.Any(symbol =>
    //         symbol.symbol == symbolValue && symbol.trackers.Any(tracker => tracker._interval == interval));

    private List<SignalTrackerSB> GetTrackersByInterval(string interval) =>
        GetAllTrackers()
            .Where(t => t._interval == interval)
            .ToList();

    private List<SignalTrackerSB> GetAllTrackers() =>
        _symbols
            .SelectMany(s => s.trackers)
            .ToList();
}