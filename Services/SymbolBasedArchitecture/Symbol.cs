namespace TradingVault.Services.SymbolBasedArchitecture;

public class Symbol
{
    public string symbol;
    public List<SignalTrackerSB> trackers;

    public Symbol(string symbol)
    {
        this.symbol = symbol;
        trackers = new List<SignalTrackerSB>();
    }

    private List<SignalTrackerSB> GetTrackersForInterval(string interval)
        => trackers.FindAll(tracker => tracker._interval == interval);

    public void StopTrackers()
    {
        foreach (var tracker in trackers) 
            tracker.Stop();
        
        trackers.Clear();
    }

    public void StopTrackersByInterval(string interval)
    {
        var trackersForInterval = GetTrackersForInterval(interval);

        foreach (var tracker in trackersForInterval)
        {
            tracker.Stop();
            trackers.Remove(tracker);
        }
    }
}