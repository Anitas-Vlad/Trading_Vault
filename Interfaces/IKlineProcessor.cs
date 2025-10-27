using TradingVault.Models;

namespace TradingVault.Interfaces;

public interface IKlineProcessor
{
    Task<List<RsiResult>> CheckRsiBellow(int rsiValue, string interval);

    Task<List<RsiResult>> CheckRsiAbove(int rsiValue, string interval);

    Task<List<string>> SelectTradingPairsForQuoteAsset();
    // Task StopKlineProcessor(CancellationToken cancellationToken);
}