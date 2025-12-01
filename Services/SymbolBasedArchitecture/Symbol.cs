using TradingVault.Requests;

namespace TradingVault.Services.SymbolBasedArchitecture;

public class Symbol
{
    public string symbol;
    public List<SignalTrackerSB> trackers;
    public List<SignalTrackerSB> specificTrackers;

    public Symbol(string symbol)
    {
        this.symbol = symbol;
        trackers = [];
        specificTrackers = [];
    }

    public void AddSpecificTracker(SignalTrackerSB trackerToAdd)
    {
        var tracker = specificTrackers.FirstOrDefault(t => t._interval == trackerToAdd._interval);
        if (tracker == null)
        {
            specificTrackers.Add(trackerToAdd);
        }
        else
        {
            tracker = trackerToAdd;
        }
    }

    public SignalTrackerSB? FindSpecificTracker(StartSpecificSymbolRsiRequest request)
        => specificTrackers.FirstOrDefault(t => t._interval == request.interval);

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