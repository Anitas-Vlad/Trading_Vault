namespace TradingVault.Models;

public class SignalTrackerKey(string symbol, string interval, int rsiValue)
{
    private readonly string _symbol = symbol;
    private readonly string _interval = interval;
    private readonly int _rsiValue = rsiValue;
}