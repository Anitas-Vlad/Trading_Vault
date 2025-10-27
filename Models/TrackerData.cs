namespace TradingVault.Models;

public class TrackerData(string interval, int rsiValue)
{
    public string Interval { get; } = interval;
    public int RsiValue { get; } = rsiValue;
}