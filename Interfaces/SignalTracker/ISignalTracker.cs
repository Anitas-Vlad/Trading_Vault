namespace TradingVault.Interfaces.SignalTracker;

public interface ISignalTracker
{
    Task InitializeBuySignalAsync();
    void Stop();
}