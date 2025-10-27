namespace TradingVault.Options.TradingOptions;

public class GeneralOptions : TradingOptions
{
    // public string QuoteAsset { get; set; } = "USDC";
    // public int RsiPeriod { get; set; } = 14;
    // public int Limit { get; set; } = 50;

    // MACD-specific
    public int FastEma { get; set; } = 12;
    public int SlowEma { get; set; } = 26;
    public int UltraSlowEma { get; set; } = 26; //TODO 
    public int SignalPeriod { get; set; } = 9;
}