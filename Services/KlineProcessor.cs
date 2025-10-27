using Microsoft.Extensions.Options;
using TradingVault.Interfaces;
using TradingVault.Models;
using TradingVault.Options;
using TradingVault.Options.TradingOptions;

namespace TradingVault.Services;

public class KlineProcessor(
    IBinanceClient binanceClient,
    IOptions<TradingOptions> tradingOptions,
    IIndicatorService indicatorService,
    ITradingAnalyzer tradingAnalyzer)
    : IKlineProcessor
{
    private readonly TradingOptions _tradingOptions = tradingOptions.Value;

    // private CancellationTokenSource _cts = new();

    public async Task<List<RsiResult>> CheckRsiBellow(int rsiValue, string interval)
    {
        var usdcPairs = await SelectTradingPairsForQuoteAsset();
        Console.WriteLine($"Checking RSI for {usdcPairs.Count} {_tradingOptions.QuoteAsset} pairs...");

        var results = await tradingAnalyzer.AnalyzeRsiAsync(usdcPairs, interval);

        var rsiValues = results
            .Where(value => value.Rsi < rsiValue)
            .OrderBy(value => value.Rsi)
            .ToList();

        return rsiValues;
    }
    
    public async Task<List<RsiResult>> CheckRsiAbove(int rsiValue, string interval)
    {
        var usdcPairs = await SelectTradingPairsForQuoteAsset();
        Console.WriteLine($"Checking RSI for {usdcPairs.Count} {_tradingOptions.QuoteAsset} pairs...");

        var results = await tradingAnalyzer.AnalyzeRsiAsync(usdcPairs, interval);

        var rsiValues = results
            .Where(value => value.Rsi > rsiValue)
            .OrderByDescending(value => value.Rsi)
            .ToList();

        return rsiValues;
    }

    public async Task<List<string>> SelectTradingPairsForQuoteAsset()
    {
        var symbols = await binanceClient.GetSymbolsAsync();
        var usdcPairs = symbols
            .Where(s => s.Status == "TRADING" &&
                        s.QuoteAsset.Equals(_tradingOptions.QuoteAsset, StringComparison.OrdinalIgnoreCase))
            .Select(s => s.Symbol)
            .ToList();
        return usdcPairs;
    }

    private void PrintLowRsiSymbols(List<RsiResult> rsiResults, int rsiTreshold)
    {
        var lowRsi = rsiResults
            .Where(result => result.Rsi < rsiTreshold)
            .OrderBy(result => result.Rsi)
            .ToList();

        if (lowRsi.Count == 0)
        {
            Console.WriteLine("No symbols with RSI.");
            return;
        }

        Console.WriteLine($"Symbols with RSI < {rsiTreshold}:");
        Console.WriteLine("----------------------");

        foreach (var rsi in lowRsi)
        {
            // Symbol left-aligned, RSI right-aligned with 2 decimals
            Console.WriteLine($"{rsi.Rsi,-10}: RSI = {rsi.Rsi,6:F2}");
        }
    }

    // public async Task StopKlineProcessor(CancellationToken cancellationToken)
    //     => await _cts.CancelAsync();
}