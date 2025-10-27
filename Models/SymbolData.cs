namespace TradingVault.Models;

public class SymbolData
{
    public string Symbol { get; set; } = string.Empty;
    public List<decimal> Closes { get; set; } = new();
}