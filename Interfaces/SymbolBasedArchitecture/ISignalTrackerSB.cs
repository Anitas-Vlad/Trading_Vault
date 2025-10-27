namespace TradingVault.Interfaces.SymbolBasedArchitecture;

public interface ISignalTrackerSB
{
    // Task InitializeBuySignalAsync();
    Task UpdateBuySignalAsync();
    Task SeedAsync();
}