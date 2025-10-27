using TradingVault.Models;

namespace TradingVault.Interfaces;

public interface ITradingAnalyzer
{
    Task<List<RsiResult>> AnalyzeRsiAsync(List<string> symbols, string interval);

    Task<List<MacdResult>> AnalyzeMacdAsync(List<string> symbols, string interval, int fastEma, int slowEma,
        int signalPeriod);

    Task<List<string>> GetTradingPairsAsync();
}