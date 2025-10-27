using Telegram.Bot.Types.ReplyMarkups;

namespace TradingVault.Interfaces;

public interface ITelegramService
{
    Task SendMessageAsync(string message);
    Task SendMessageAsync(string message, InlineKeyboardMarkup replyMarkup);
    Task<string?> ListenForCommands();
    Task InitializeLastUpdateId();
}