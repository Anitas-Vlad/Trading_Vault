using Microsoft.Extensions.Options;
using TradingVault.Interfaces;
using TradingVault.Options;
using TradingVault.Options.TradingOptions;

namespace TradingVault.Services;

public class SymbolService : ISymbolService
{
    private readonly IBinanceClient _binance;
    private readonly TradingOptions _trading;

    public SymbolService(IBinanceClient binance, IOptions<TradingOptions> trading)
    {
        _binance = binance;
        _trading = trading.Value;
    }

    public async Task<List<string>> GetTradableSymbolsAsync(CancellationToken ct = default)
    {
        var all = await _binance.GetSymbolsAsync();
        return all
            .Where(s => s.Status == "TRADING" && s.QuoteAsset.Equals(_trading.QuoteAsset, StringComparison.OrdinalIgnoreCase))
            .Select(s => s.Symbol)
            .OrderBy(s => s)
            .ToList();
    }
}