using TradingVault.Models.Enums;

namespace TradingVault.Responses;

public class BinanceCandleProcessorInfo
{
    public string Symbol { get; set; }
    public string TimeSpan { get; set; }
    public long LastCandleCloseTime { get; set; }
    public TradeSignal TradeSignal { get; set; }

    public BinanceCandleProcessorInfo(string symbol, string timeSpan, long lastCandleCloseTime, TradeSignal tradeSignal)
    {
        Symbol = symbol;
        TimeSpan = timeSpan;
        LastCandleCloseTime = lastCandleCloseTime;
        TradeSignal = tradeSignal;
    }
}