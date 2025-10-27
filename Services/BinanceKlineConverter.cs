using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using TradingVault.Models;

namespace TradingVault.Services;

public class BinanceKlineConverter : JsonConverter<BinanceKline>
{
    public override BinanceKline? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException();

        reader.Read();
        var openTime = reader.GetInt64();
        reader.Read();
        var open = decimal.Parse(reader.GetString()!, CultureInfo.InvariantCulture);
        reader.Read();
        var high = decimal.Parse(reader.GetString()!, CultureInfo.InvariantCulture);
        reader.Read();
        var low = decimal.Parse(reader.GetString()!, CultureInfo.InvariantCulture);
        reader.Read();
        var close = decimal.Parse(reader.GetString()!, CultureInfo.InvariantCulture);
        reader.Read();
        var volume = decimal.Parse(reader.GetString()!, CultureInfo.InvariantCulture);
        reader.Read();
        var closeTime = reader.GetInt64();
        reader.Read();
        var quoteAssetVolume = decimal.Parse(reader.GetString()!, CultureInfo.InvariantCulture);
        reader.Read();
        var trades = reader.GetInt32();
        reader.Read();
        var takerBuyBase = decimal.Parse(reader.GetString()!, CultureInfo.InvariantCulture);
        reader.Read();
        var takerBuyQuote = decimal.Parse(reader.GetString()!, CultureInfo.InvariantCulture);
        reader.Read();
        var ignore = decimal.Parse(reader.GetString()!, CultureInfo.InvariantCulture);
       
        // var isClosed = bool.TryParse(reader.GetString(), out var closed) && closed;
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var isClosed = closeTime < now;

        reader.Read(); // EndArray

        return new BinanceKline
        {
            OpenTime = openTime,
            Open = open,
            High = high,
            Low = low,
            Close = close,
            Volume = volume,
            CloseTime = closeTime,
            QuoteAssetVolume = quoteAssetVolume,
            NumberOfTrades = trades,
            TakerBuyBaseVolume = takerBuyBase,
            TakerBuyQuoteVolume = takerBuyQuote,
            Ignore = ignore,
            IsClosed = isClosed
        };
    }

    public override void Write(Utf8JsonWriter writer, BinanceKline value, JsonSerializerOptions options)
    {
        throw new NotImplementedException(); // We don’t need to serialize back to Binance
    }
}