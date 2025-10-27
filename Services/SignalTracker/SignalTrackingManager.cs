using TradingVault.Interfaces.SignalTracker;

namespace TradingVault.Services.SignalTracker;

public class SignalTrackingManager : ISignalTrackingManager
{
    private readonly List<SignalTracker> _trackers = [];

    public async Task AddTracker(SignalTracker tracker)
    {
        _trackers.Add(tracker);
        Task.Run(async () => await tracker.InitializeBuySignalAsync());
    }

    public void StopAll()
    {
        foreach (var tracker in _trackers)
            tracker.Stop();

        _trackers.Clear();
    }

    public void StopTracker(string symbol, string interval, int rsiTreshold)
    {
        var tracker = _trackers
            .FirstOrDefault(tracker =>
                tracker._symbol == symbol && tracker._interval == interval && tracker._rsiValue == rsiTreshold);

        if (tracker == null)
            return;
        
        tracker.Stop();
        _trackers.Remove(tracker);
    }
}