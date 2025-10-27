namespace TradingVault.Interfaces.SignalTracker;

public interface ISignalTrackingManager
{
    Task AddTracker(Services.SignalTracker.SignalTracker tracker);
    void StopAll();
}