namespace TradingVault.Interfaces;

public interface ISymbolService
{
    Task<List<string>> GetTradableSymbolsAsync(CancellationToken ct = default);
}