using TradingVault.Interfaces;
using TradingVault.Models;

namespace TradingVault.Services;

public class OutputService(ITelegramService telegramService) : IOutputService
{
    public async Task HandleRsiBelow(List<RsiResult> results, string interval, int rsiValue)
    {
        if (results.Count == 0)
            await telegramService.SendMessageAsync($"No {interval} symbols found with Rsi < {rsiValue}.");

        foreach (var result in results)
        {
            // Symbol left-aligned, RSI right-aligned with 2 decimals
            await telegramService.SendMessageAsync($"{result.Symbol,-10}: RSI = {result.Rsi,6:F2}");
        }
    }

    public async Task HandleRsiAbove(List<RsiResult> results, string interval, int rsiValue)
    {
        if (results.Count == 0)
            await telegramService.SendMessageAsync($"No {interval} symbols found with Rsi > {rsiValue}.");

        foreach (var result in results)
        {
            // Symbol left-aligned, RSI right-aligned with 2 decimals
            await telegramService.SendMessageAsync($"{result.Symbol,-10}: RSI = {result.Rsi,6:F2}");
        }
    }
}