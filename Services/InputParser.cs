using TradingVault.Interfaces;
using TradingVault.Models;
using TradingVault.Requests;

namespace TradingVault.Services;

public class InputParser : IInputParser
{
    private HashSet<string> _validIntervals =
    [
        "1s", "1m", "3m", "5m", "15m", "30m",
        "1h", "2h", "4h", "6h", "8h", "12h",
        "1d", "3d",
        "1w",
        "1M"
    ];

    public TrackerData? ParseStartTrackerCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 4 || !parts[0].Equals("start", StringComparison.OrdinalIgnoreCase) ||
            !parts[1].Equals("trackers", StringComparison.OrdinalIgnoreCase))
            return null;

        var interval = parts[2].Trim();
        var rsiString = parts[3].Trim();

        if (!_validIntervals.Contains(interval))
            return null;

        if (!int.TryParse(rsiString, out var rsiValue) || rsiValue < 1 || rsiValue > 100)
            return null;

        return new TrackerData(interval, rsiValue);
    }

    public string? ParseStopTrackingIntervalCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length != 3 || !parts[0].Equals("stop", StringComparison.OrdinalIgnoreCase) || !parts[1].Equals("tracking", StringComparison.OrdinalIgnoreCase))
            return null;
        
        var interval = parts[2].Trim();
        
        return !_validIntervals.Contains(interval) ? null : interval;
    }

    public RsiRequest? ParseRsiCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        // Normalize input (case insensitive, trimmed)
        input = input.Trim();

        // Example: "rsi below 30 5m"
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 4)
            return null;

        if (parts[0] != "rsi" || parts[1] != "above")
            return null;

        if (!int.TryParse(parts[2], out var rsiValue))
            return null;

        var interval = parts[3];
        if (!_validIntervals.Contains(interval))
            return null;

        return new RsiRequest
        {
            RsiValue = rsiValue,
            Interval = interval
        };
    }
}