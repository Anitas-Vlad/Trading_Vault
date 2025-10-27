using TradingVault.Models;

namespace TradingVault.Services;

public interface IOutputService
{
    Task HandleRsiBelow(List<RsiResult> results, string interval, int rsiValue);
    Task HandleRsiAbove(List<RsiResult> results, string interval, int rsiValue);
}