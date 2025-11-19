using Microsoft.Extensions.Options;
using TradingVault.Interfaces;
using TradingVault.Interfaces.SymbolBasedArchitecture;
using TradingVault.Models;
using TradingVault.Options;
using TradingVault.Options.TradingOptions;

namespace TradingVault.Services.SymbolBasedArchitecture;

public class SignalTrackerSB(
    string symbol, //TODO Remove
    string interval,
    int rsiValue,
    IIndicatorService indicatorService,
    IBinanceClient binanceClient,
    ITelegramService telegramService,
    IOptions<TradingOptions> tradingOptions)
    : ISignalTrackerSB
{
    public string _symbol = symbol;
    public string _interval = interval;
    public int _rsiValue = rsiValue;
    public CancellationTokenSource _cts = new();
    private List<decimal> _closes = [];
    private List<decimal> _volumes = [];
    private decimal? _previewClose;
    private bool IsInitiation = true;
    private bool IsInitialized = false;

    private TradingOptions _tradingOptions = tradingOptions.Value;

    public async Task SeedAsync()
    {
        var klines = await binanceClient.GetKlinesAsync(_symbol, interval, _tradingOptions.Limit, default);

        // Keep only closed candles for history
        var closedKlines = klines.Where(k => k.IsClosed).ToList();

        _closes.Clear();
        _volumes.Clear();

        foreach (var k in closedKlines)
        {
            _closes.Add(k.Close);
            _volumes.Add(k.Volume);
        }
    }

    public async Task UpdateBuySignalAsync()
    {
        try
        {
            BinanceKline? lastKline;

            if (IsInitiation)
            {
                lastKline = await binanceClient.GetLastKline(_symbol, _interval);
                IsInitiation = false;
                IsInitialized = true;
            }
            else
                lastKline = await binanceClient.GetLastClosedKline(_symbol, _interval);

            if (lastKline == null)
            {
                Console.WriteLine($"⚠️ No candle for {_symbol} [{_interval}]");
                return;
            }

            var lastClose = lastKline.Close;
            var lastVolume = lastKline.Volume;

            if (_closes.Count == 0 || _closes.Last() != lastClose)
                UpdateClosesAndVolumes(lastClose, lastVolume);
            else
                Console.WriteLine($"🔁 {_symbol} [{_interval}] No new candle (same close).");

            await EvaluateBuySignalAsync();
        }
        catch (TaskCanceledException)
        {
            await telegramService.SendMessageAsync($"🛑 Tracker for {_symbol} [{_interval}] stopped.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error updating tracker {_symbol} [{_interval}]: {ex.Message}");
        }
    }

    private void UpdateClosesAndVolumes(decimal lastClose, decimal lastVolume)
    {
        if (IsInitialized)
        {
            _closes.RemoveAt(_closes.Count - 1);
            _volumes.RemoveAt(_volumes.Count - 1);
            IsInitialized = false;
        }

        if (_closes.Count >= _tradingOptions.Limit)
        {
            _closes.RemoveAt(0);
            _volumes.RemoveAt(0);
        }

        _closes.Add(lastClose);
        _volumes.Add(lastVolume);
    }

    private async Task EvaluateBuySignalAsync()
    {
        // Make sure we have enough data for indicators
        if (_closes.Count < Math.Max(_tradingOptions.SlowEma, _tradingOptions.UltraSlowEma))
            return;

        // === 1. RSI ===
        var rsi = indicatorService.ComputeRsi(_closes);

        // === 2. MACD ===
        var macd = indicatorService.ComputeMacd(
            _closes,
            _tradingOptions.FastEma,
            _tradingOptions.SlowEma,
            _tradingOptions.SignalPeriod);

        var last = macd.MacdLine.Count - 1;
        
        var crossedUp =
            macd.MacdLine[last - 1] < macd.SignalLine[last - 1] && // was below
            macd.MacdLine[last] > macd.SignalLine[last]; // crossed above

        // === 3. OBV ===
        var obvSeries = indicatorService.ComputeOnBalanceVolume(_closes, _volumes);
        var obvTrendUp = obvSeries[^1] > obvSeries[^2];

        // === 4. UltraSlowEma trend filter ===
        var emaSeries = indicatorService.ComputeEma(_closes, _tradingOptions.UltraSlowEma);
        if (emaSeries.Count < 2) return;

        var ultraSlowEma = emaSeries[^1];
        var prevUltraSlowEma = emaSeries[^2];
        var isTrendUp = ultraSlowEma > prevUltraSlowEma;
        var isRsiUnderHighLimit = rsi < _rsiValue;

        // Hardcoded Rsi Limits
        var isRsiBetweenLimits = rsi > _tradingOptions.LowRsi & rsi < _tradingOptions.HighRsi;

        // === Final BUY condition ===
        if (crossedUp) // crossedUp && isTrendUp && rsi < _rsiValue && obvTrendUp && isRsiBetweenLimits  && isRsiUnderHighLimit
        {
            await telegramService.SendMessageAsync(
                $"✅ BUY signal for {_symbol} [{_interval}] " +
                $"(RSI={rsi:F2}, MACD crossover, OBV up, EMA{_tradingOptions.UltraSlowEma} trending up)"
            );
        }

        Console.WriteLine($"SUCCESS: {_symbol} - interval: {_interval}");
    }

    public void Stop() => _cts.Cancel();
}