namespace TradingVault.Options.TradingOptions;

public class ScalpingOptions : TradingOptions
{
    
    // public string QuoteAsset { get; set; } = "USDC";
    // public int RsiPeriod { get; set; } = 14;
    // public int Limit { get; set; } = 50;

    // MACD-specific
    public int FastEma { get; set; } = 8;
    public int SlowEma { get; set; } = 21;
    public int UltraSlowEma { get; set; } = 21;
    public int SignalPeriod { get; set; } = 5;
}