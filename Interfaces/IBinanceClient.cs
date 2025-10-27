using TradingVault.Models;

namespace TradingVault.Interfaces;

public interface IBinanceClient
{
    Task<List<SymbolInfo>> GetSymbolsAsync(CancellationToken ct = default);
    Task<decimal> GetCurrentPriceForSymbol(string symbol);

    Task<List<BinanceKline>> GetKlinesAsync(
        string symbol,
        string interval,
        int limit,
        CancellationToken ct);

    Task<List<decimal>> GetVolumesAsync(
        string symbol, string interval, int limit);

    Task<List<decimal>> GetClosesAsync(string symbol, int limit, string interval);
    Task<decimal> GetLastCloseAsync(string symbol, string interval);

    Task<BinanceKline?> GetLastKline(string symbol,
        string interval);

    Task<BinanceKline?> GetLastClosedKline(string symbol, string interval);
    Task<List<BinanceKline>> GetLast2Klines(string symbol, string interval);
}