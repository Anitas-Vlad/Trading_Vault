namespace TradingVault.Interfaces.SignalTracker;

public interface ISignalTrackerFactory
{
    Services.SignalTracker.SignalTracker Create(string symbol, string interval, int rsiValue);
    Task CreateTrackersForUsdcPairsAsync(string interval, int rsiValue);
    Task RequestActiveTrackersCount();
    void StopAll();
    void StopTrackersForInterval(string interval);
}