using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TradingVault.Interfaces;
using TradingVault.Models;
using TradingVault.Options;
using TradingVault.Options.TradingOptions;

namespace TradingVault.Services;

public class BinanceClient : IBinanceClient
{
    private readonly HttpClient _httpClient;
    private readonly BinanceOptions _binanceOptions;
    private readonly TradingOptions _tradingOptions;

    public BinanceClient(HttpClient httpClient, IOptions<BinanceOptions> binanceOptions,
        IOptions<TradingOptions> tradingOptions)
    {
        _httpClient = httpClient;
        _binanceOptions = binanceOptions.Value;
        _tradingOptions = tradingOptions.Value;
        if (!string.IsNullOrWhiteSpace(_binanceOptions.BaseUrl))
            _httpClient.BaseAddress = new Uri(_binanceOptions.BaseUrl);
    }

    public async Task<decimal> GetCurrentPriceForSymbol(string symbol)
    {
        var url = $"{_binanceOptions.BaseUrl}/api/v3/ticker/price?symbol={symbol.ToUpper()}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        var priceString = doc.RootElement.GetProperty("price").GetString();

        if (decimal.TryParse(priceString, out var price))
            return price;

        throw new Exception($"Unable to parse price for symbol {symbol}");
    }

    public async Task<List<SymbolInfo>> GetSymbolsAsync(CancellationToken ct = default)
    {
        using var resp = await _httpClient.GetAsync(_binanceOptions.ExchangeInfoPath, ct);
        resp.EnsureSuccessStatusCode();
        var stream = await resp.Content.ReadAsStreamAsync(ct);
        var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        return doc.RootElement.GetProperty("symbols").EnumerateArray().Select(s => new SymbolInfo
        {
            Symbol = s.GetProperty("symbol").GetString()!,
            BaseAsset = s.GetProperty("baseAsset").GetString()!,
            QuoteAsset = s.GetProperty("quoteAsset").GetString()!,
            Status = s.GetProperty("status").GetString()!
        }).ToList();
    }

    public async Task<List<BinanceKline>>
        GetKlinesAsync(string symbol, string interval, int limit, CancellationToken ct = default)
    {
        var url = $"{_binanceOptions.BaseUrl}/api/v3/klines?symbol={symbol}&interval={interval}&limit={limit}";
        var stream = await _httpClient.GetStreamAsync(url, ct);

        var options = new JsonSerializerOptions();
        options.Converters.Add(new BinanceKlineConverter());

        var klines = await JsonSerializer.DeserializeAsync<List<BinanceKline>>(stream, options, ct);
        return klines
            // .Where(k => k.IsClosed)
            .ToList();
    }

    public async Task<BinanceKline?> GetLastKline(string symbol,
        string interval)
    {
        var lastKline = (await GetKlinesAsync(symbol, interval, 1)).FirstOrDefault();
        if (lastKline == null)
            throw new Exception($"Unable to get last kline {symbol}");
        return lastKline;
    }
    
    public async Task<BinanceKline?> GetLastClosedKline(string symbol, string interval)
    {
        var klines = await GetKlinesAsync(symbol, interval, 2);

        if (klines == null || klines.Count < 2)
            throw new Exception($"Unable to get klines for {symbol} {interval}");

        // Take the previous candle (last closed one)
        var lastClosedCandle = klines[^2];

        return lastClosedCandle;
    }

    public async Task<List<BinanceKline>> GetLast2Klines(string symbol, string interval)
    {
        var klines = await GetKlinesAsync(symbol, interval, 2);

        if (klines == null || klines.Count < 2)
            throw new Exception($"Unable to get klines for {symbol} {interval}");

        return klines.OrderBy(k => k.OpenTime).ToList();
    }

    public async Task<List<decimal>>
        GetClosesAsync(string symbol, int limit,
            string interval) //TODO Refactor so you don't fetch twice, once for volumes and once for Closes. Just use the GetKlines once.
    {
        var klines = await GetKlinesAsync(symbol, interval, limit);
        return klines
            .Select(k => k.Close)
            .ToList();
    }

    public async Task<List<decimal>>
        GetVolumesAsync( //TODO Refactor so you don't fetch twice, once for volumes and once for Closes. Just use the GetKlines once. 
            string symbol, string interval, int limit)
    {
        var klines = await GetKlinesAsync(symbol, interval, limit);
        return klines.Select(k => k.Volume).ToList();
    }

    public async Task<decimal>
        GetLastCloseAsync(string symbol,
            string interval) //TODO Refactor so you don't fetch twice, once for volumes and once for Closes. Just use the GetKlines once.
    {
        var klines = await GetKlinesAsync(symbol, interval, 1);
        return klines.Select(kline => kline.Close).FirstOrDefault();
    }
}