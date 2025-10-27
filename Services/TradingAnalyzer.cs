using Microsoft.Extensions.Options;
using TradingVault.Interfaces;
using TradingVault.Models;
using TradingVault.Options;
using TradingVault.Options.TradingOptions;

namespace TradingVault.Services;

public class TradingAnalyzer(
    IBinanceClient binanceClient,
    IIndicatorService indicatorService,
    IOptions<TradingOptions> tradingOptions)
    : ITradingAnalyzer
{
    private readonly TradingOptions _tradingOptions = tradingOptions.Value;

    // --------------------- RSI Methods ---------------------

    public async Task<List<RsiResult>> AnalyzeRsiAsync(List<string> symbols, string interval)
    {
        var tasks = symbols.Select(async symbol =>
        {
            try
            {
                var closes = await binanceClient.GetClosesAsync(symbol, _tradingOptions.RsiPeriod + 1, interval);
                if (closes.Count < _tradingOptions.RsiPeriod)
                    return null;

                var rsi = indicatorService.ComputeRsi(closes, _tradingOptions.RsiPeriod);

                return new RsiResult
                {
                    Symbol = symbol,
                    Rsi = rsi
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing {symbol}: {ex.Message}");
                return null;
            }
        });

        var results = await Task.WhenAll(tasks);
        return results.Where(r => r != null).ToList()!;
    }



    // --------------------- MACD Methods ---------------------

    public async Task<List<MacdResult>> AnalyzeMacdAsync(List<string> symbols, string interval, int fastEma = 12,
        int slowEma = 26, int signalPeriod = 9)
    {
        var tasks = symbols.Select(async symbol =>
        {
            var closes = await binanceClient.GetClosesAsync(symbol, slowEma + signalPeriod, interval);
            if (closes.Count < slowEma + signalPeriod)
                return null;

            var macdResult = indicatorService.ComputeMacd(closes, fastEma, slowEma, signalPeriod);
            return macdResult;
        });

        var results = await Task.WhenAll(tasks);
        return results.Where(r => r != null).ToList()!;
    }

    // --------------------- Shared Methods ---------------------

    public async Task<List<string>> GetTradingPairsAsync()
    {
        var symbols = await binanceClient.GetSymbolsAsync();
        return symbols
            .Where(s => s.Status == "TRADING" &&
                        s.QuoteAsset.Equals(_tradingOptions.QuoteAsset, StringComparison.OrdinalIgnoreCase))
            .Select(s => s.Symbol)
            .ToList();
    }
}