using Microsoft.Extensions.Options;
using Telegram.Bot.Types.ReplyMarkups;
using TradingVault.Interfaces;
using TradingVault.Interfaces.SignalTracker;
using TradingVault.Options;
using TradingVault.Options.TradingOptions;

namespace TradingVault.Services.SignalTracker;

public class SignalTracker(
    string symbol,  //TODO Remove
    string interval,
    int rsiValue,
    IIndicatorService indicatorService,
    IBinanceClient binanceClient,
    ITelegramService telegramService,
    IOptions<TradingOptions> tradingOptions)
    : ISignalTracker
{
    public string _symbol = symbol;
    public string _interval = interval;
    public int _rsiValue = rsiValue;
    public CancellationTokenSource _cts = new();
    private List<decimal> _closes = [];
    private List<decimal> _volumes = [];
    private decimal? _previewClose;

    private TradingOptions _tradingOptions = tradingOptions.Value;

    public async Task InitializeBuySignalAsync() // TODO bool isTemporary
        //TODO Refactor so it doesn't update the first time right after starting the tracker
    {
        Console.WriteLine($"📈 Starting tracker for {_symbol} [{_interval}] with RSI < {_rsiValue}");

        try
        {
            _closes = await binanceClient.GetClosesAsync(_symbol, 101, _interval); //TODO Change Limit
            _volumes = await binanceClient.GetVolumesAsync(_symbol, _interval, 101);

            var rsi = indicatorService.ComputeRsi(_closes);
            var macd = indicatorService.ComputeMacd(_closes, _tradingOptions.FastEma, _tradingOptions.SlowEma,
                _tradingOptions.SignalPeriod);

            await EvaluateBuySignalAsync();
        }
        catch (TaskCanceledException)
        {
            await telegramService.SendMessageAsync($"🛑 Tracker for {_symbol} [{_interval}] stopped.");
            //TODO check if you need to drop the tracker out of the _trackers
        }
    }

    public async Task UpdateBuySignalAsync()
    {
        try
        {
            var lastCandle = await binanceClient.GetLastKline(_symbol, _interval);

            var lastClose = lastCandle!.Close;
            var lastVolume = lastCandle!.Volume;
            // var lastClose = await binanceClient.GetLastCloseAsync(_symbol, _interval);

            if (_closes.Count == 0 || _closes.Last() != lastClose)
            {
                if (_closes.Count >= _tradingOptions.Limit)
                {
                    _closes.RemoveAt(0);
                    _volumes.RemoveAt(0);
                }

                _closes.Add(lastClose);
                _volumes.Add(lastVolume);
            }

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

        // === Final BUY condition ===
        if (rsi < _rsiValue && crossedUp && isTrendUp)
            // && obvTrendUp
        {
            await telegramService.SendMessageAsync(
                $"✅ BUY signal for {_symbol} [{_interval}] " +
                $"(RSI={rsi:F2}, MACD crossover, OBV up, EMA{_tradingOptions.UltraSlowEma} trending up)"
            );
            // await SendBuySignalAsync(
            //     $"✅ BUY signal for {_symbol} [{_interval}] " +
            //     $"(RSI={rsi:F2}, MACD crossover, OBV up, EMA{_tradingOptions.UltraSlowEma} trending up)");
        }

        Console.WriteLine($"SUCCESS: {_symbol} - interval: {_interval}");
    }

    // private async Task EvaluateBuySignalAsync()
    // {
    //     if (_closes.Count < Math.Max(_tradingOptions.SlowEma, _tradingOptions.UltraSlowEma))
    //         return;
    //
    //     var rsiOk = IsRsiOversold(_closes, _rsiValue);
    //     var macdOk = IsMacdBullish(_closes, _tradingOptions.FastEma, _tradingOptions.SlowEma,
    //         _tradingOptions.SignalPeriod);
    //     var obvOk = IsObvTrendUp(_closes, _volumes);
    //     var emaOk = IsUltraSlowEmaUp(_closes, _tradingOptions.UltraSlowEma);
    //
    //     if (rsiOk && macdOk && obvOk && emaOk)
    //     {
    //         // await telegramService.SendMessageAsync(
    //         //     $"✅ BUY signal for {_symbol} [{_interval}] " +
    //         //     $"(RSI<={_rsiValue}, MACD crossover, OBV up, EMA{_tradingOptions.UltraSlowEma} trending up)"
    //         // );
    //
    //         await SendBuySignalAsync($"✅ BUY signal for {_symbol} [{_interval}] " +
    //                                  $"(RSI<={_rsiValue}, MACD crossover, OBV up, EMA{_tradingOptions.UltraSlowEma} trending up)");
    //         await telegramService.SendMessageAsync(
    //             
    //             $"✅ BUY signal for {_symbol} [{_interval}] " +
    //             $"(RSI={rsi:F2}, MACD crossover, OBV up, EMA{_tradingOptions.UltraSlowEma} trending up)"
    //         );
    //     }
    // }

    private async Task SendBuySignalAsync(string message)
    {
        var url = $"https://www.binance.com/en/trade/{_symbol.Replace("USDC", "_USDC")}";
        var replyMarkup = new InlineKeyboardMarkup(
            InlineKeyboardButton.WithUrl("📈 Open in Binance", url)
        );

        await telegramService.SendMessageAsync(message, replyMarkup);
    }

    private bool IsRsiOversold(List<decimal> closes, decimal threshold)
    {
        var rsi = indicatorService.ComputeRsi(closes);
        return rsi < threshold;
    }

    private bool IsMacdBullish(List<decimal> closes, int fast, int slow, int signal)
    {
        var macd = indicatorService.ComputeMacd(closes, fast, slow, signal);

        if (macd.MacdLine.Count < 2 || macd.SignalLine.Count < 2)
            return false;

        var last = macd.MacdLine.Count - 1;
        return macd.MacdLine[last - 1] < macd.SignalLine[last - 1] &&
               macd.MacdLine[last] > macd.SignalLine[last];
    }

    private bool IsObvTrendUp(List<decimal> closes, List<decimal> volumes)
    {
        if (closes.Count < 2 || volumes.Count < 2 || closes.Count != volumes.Count)
            return false;

        var obvSeries = indicatorService.ComputeOnBalanceVolume(closes, volumes);
        return obvSeries[^1] > obvSeries[^2];
    }

    private bool IsUltraSlowEmaUp(List<decimal> closes, int period)
    {
        var emaSeries = indicatorService.ComputeEma(closes, period);
        if (emaSeries.Count < 2) return false;

        return emaSeries[^1] > emaSeries[^2];
    }

    public void Stop() => _cts.Cancel();
}