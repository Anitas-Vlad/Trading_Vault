namespace TradingVault.Requests;

public class StartSpecificSymbolRsiRequest
{
    public string symbol { get; set; }
    public string interval { get; set; }
    public int rsi { get; set; }
}