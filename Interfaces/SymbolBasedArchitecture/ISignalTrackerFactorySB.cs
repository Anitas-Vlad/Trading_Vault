using TradingVault.Services.SymbolBasedArchitecture;

namespace TradingVault.Interfaces.SymbolBasedArchitecture;

public interface ISignalTrackerFactorySB
{
    SignalTrackerSB CreateTracker(string symbol, string interval, int rsiThreshold);
    Task CreateTrackersForUsdcPairsAsync(string interval, int rsiValue);
    void StopAllTrackers();
    Task StopTrackersForInterval(string interval);
    Task RequestActiveTrackersCount();
}