namespace TradingVault.Models;

public class SymbolInfo
{
    public string Symbol { get; set; } = string.Empty;
    public string BaseAsset { get; set; } = string.Empty;
    public string QuoteAsset { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // TRADING, etc.
}