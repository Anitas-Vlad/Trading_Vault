using TradingVault.Models;

namespace TradingVault.Interfaces;

public interface IIndicatorService
{
    List<decimal> ComputeOnBalanceVolume(List<decimal> closes, List<decimal> volumes);
    decimal ComputeRsi(IReadOnlyList<decimal> closes, int period = 14);
    MacdResult ComputeMacd(List<decimal> closes, int fastEmaPeriod, int slowEmaPeriod, int signalPeriod);
    List<decimal> ComputeEma(List<decimal> closes, int period);
}