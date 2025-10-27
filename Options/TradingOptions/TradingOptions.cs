namespace TradingVault.Options.TradingOptions;

public class TradingOptions
{
    public string QuoteAsset { get; set; } = "USDC";
    public int RsiPeriod { get; set; } = 14;
    public int Limit { get; set; } = 50;

    // MACD-specific
    public int FastEma { get; set; } = 12;
    public int SlowEma { get; set; } = 26;
    public int UltraSlowEma { get; set; } = 48;
    public int SignalPeriod { get; set; } = 9;
}

// Trading Style	    Fast EMA	Slow EMA	Signal Line	Best For

// Scalping	            5	        13	        4	                    Very short-term moves
// Day Trading	        8	        17       	6	                    Intraday momentum
// Swing Trading	    12	        26	        9                     	Multi-day trends
// Position Trading	    19	        39      	14	                    Long-term trends


//     | Timeframe   | Typical Duration | Suitable For               | RSI Zones               | Target % Move |
//     | ----------- | ---------------- | -------------------------- | ----------------------- | ------------- |
//     | **1m–5m**   | Minutes          | Scalping                   | RSI 40–60 entry         | 0.5–1%        |
//     | **15m–30m** | 0.5–2 hours      | Fast day trades            | RSI 40–55 entry         | 1–2%          |
//     | **1h**      | Several hours    | Day trading or short swing | RSI 45–55 entry         | 2–5%          |
//     | **4h+**     | Multi-day        | Swing trading              | RSI 30/70 more relevant | 5–10%+        |
