namespace TradingVault.Requests;

public class RsiRequest
{
    public int RsiValue { get; set; }
    public string Interval { get; set; } = string.Empty;
}