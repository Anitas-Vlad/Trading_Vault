using TradingVault.Models;
using TradingVault.Requests;

namespace TradingVault.Interfaces;

public interface IInputParser
{
    RsiRequest? ParseRsiCommand(string input);
    TrackerData? ParseStartTrackerCommand(string input);
    string? ParseStopTrackingIntervalCommand(string input);
}