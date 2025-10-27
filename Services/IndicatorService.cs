using TradingVault.Interfaces;
using TradingVault.Models;

namespace TradingVault.Services;

public class IndicatorService : IIndicatorService
{
    public List<decimal> ComputeOnBalanceVolume(List<decimal> closes, List<decimal> volumes)
    {
        if (closes == null || volumes == null || closes.Count != volumes.Count)
            throw new ArgumentException("Closes and volumes must be non-null and have the same length.");

        var obv = new List<decimal> { 0m }; // start at 0 for the first point

        for (int i = 1; i < closes.Count; i++)
        {
            if (closes[i] > closes[i - 1])
            {
                obv.Add(obv.Last() + volumes[i]);
            }
            else if (closes[i] < closes[i - 1])
            {
                obv.Add(obv.Last() - volumes[i]);
            }
            else
            {
                obv.Add(obv.Last()); // unchanged
            }
        }

        return obv;
    }

    public decimal ComputeRsi(IReadOnlyList<decimal> closes, int period = 14)
    {
        if (closes == null || closes.Count < period + 1)
            throw new ArgumentException("Need at least period+1 closes to compute RSI");

        decimal gain = 0, loss = 0;
        for (var i = 1; i <= period; i++)
        {
            var diff = closes[i] - closes[i - 1];
            if (diff > 0) gain += diff;
            else loss -= diff;
        }

        var avgGain = gain / period;
        var avgLoss = loss / period;

        for (var i = period + 1; i < closes.Count; i++)
        {
            var diff = closes[i] - closes[i - 1];
            avgGain = ((avgGain * (period - 1)) + Math.Max(diff, 0)) / period;
            avgLoss = ((avgLoss * (period - 1)) + Math.Max(-diff, 0)) / period;
        }

        var rs = avgLoss == 0 ? decimal.MaxValue : avgGain / avgLoss;
        var rsi = 100 - (100 / (1 + rs));
        return Math.Clamp(rsi, 0m, 100m);
    }

    public MacdResult ComputeMacd(List<decimal> closes, int fastEmaPeriod, int slowEmaPeriod, int signalPeriod)
    {
        if (closes == null || closes.Count < slowEmaPeriod)
            throw new ArgumentException("Not enough data to compute MACD.");

        // Compute full EMAs
        var fastEma = ComputeEma(closes, fastEmaPeriod);
        var slowEma = ComputeEma(closes, slowEmaPeriod);

        // Compute MACD line for each point
        var macdLine = fastEma.Zip(slowEma, (f, s) => f - s).ToList();

        // Compute signal line EMA of MACD line
        var signalLine = ComputeEma(macdLine, signalPeriod);

        // Compute histogram
        var histogram = macdLine.Zip(signalLine, (m, s) => m - s).ToList();

        return new MacdResult
        {
            MacdLine = macdLine,
            SignalLine = signalLine,
            Histogram = histogram
        };
    }

    public List<decimal> ComputeEma(List<decimal> values, int period)
    {
        if (values.Count < period)
            throw new ArgumentException("Not enough data to compute EMA.");

        var ema = new List<decimal>();
        var sma = values.Take(period).Average();
        ema.Add(sma);

        var multiplier = 2m / (period + 1);

        for (int i = period; i < values.Count; i++)
        {
            var emaToday = (values[i] - ema.Last()) * multiplier + ema.Last();
            ema.Add(emaToday);
        }

        // Pad the beginning to align with original list length
        var padding = Enumerable.Repeat(ema.First(), period - 1).ToList();
        padding.AddRange(ema);
        return padding;
    }
}