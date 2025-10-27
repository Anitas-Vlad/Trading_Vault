namespace TradingVault.Services.SignalTracker;

public class SignalTrackerRegistry
{
    private readonly Dictionary<string, SignalTracker> _trackers = new();

    // Key format: symbol|interval|rsiValue
    public string GetKey(string symbol, string interval, int rsiValue)
        => $"{symbol}|{interval}|{rsiValue}";

    public bool TryGetTracker(string symbol, string interval, int rsiValue, out SignalTracker tracker)
    {
        var key = GetKey(symbol, interval, rsiValue);
        return _trackers.TryGetValue(key, out tracker);
    }

    public void AddTracker(SignalTracker tracker, string symbol, string interval, int rsiValue)
    {
        var key = GetKey(symbol, interval, rsiValue);
        _trackers[key] = tracker;
    }

    public IEnumerable<SignalTracker> GetAllTrackers() => _trackers.Values;
}