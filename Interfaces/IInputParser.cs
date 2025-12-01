using TradingVault.Models;
using TradingVault.Requests;

namespace TradingVault.Interfaces;

public interface IInputParser
{
    RsiRequest? ParseRsiCommand(string input);
    TrackerData? ParseStartTrackersCommand(string input);
    string? ParseStopTrackingIntervalCommand(string input);
    StartSpecificSymbolRsiRequest? ParseStartSpecificTrackerCommand(string input);
}