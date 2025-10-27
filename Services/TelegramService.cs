using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TradingVault.Interfaces;
using TradingVault.Responses.TelegramMessageResponse;

namespace TradingVault.Services;

public class TelegramService : ITelegramService
{
    private static int _lastUpdateId;
    private readonly TelegramBotClient _botClient;
    private readonly HttpClient _httpClient;
    private readonly string _telegramApiUrl;
    private readonly string _telegramChatId;

    public TelegramService(IConfiguration configuration)
    {
        var botToken = configuration["TelegramSettings:TelegramBotToken"];
        _telegramApiUrl = configuration["TelegramSettings:TelegramApiUrl"];
        _telegramChatId = configuration["TelegramSettings:TelegramChatId"];
        
        if (string.IsNullOrEmpty(botToken))
            throw new InvalidOperationException("Telegram bot token is missing from configuration.");

        _botClient = new TelegramBotClient(botToken);
        _httpClient = new HttpClient();
    }

    public async Task SendMessageAsync(string message)
    {
        try
        {
            await _botClient.SendMessage(_telegramChatId, message);
            // Console.WriteLine("SENT --- " + message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending Telegram message: {ex.Message}");
        }
    }
    
    public async Task SendMessageAsync(string message, InlineKeyboardMarkup replyMarkup)
    {
        try
        {
            await _botClient.SendMessage(
                chatId: _telegramChatId,
                text: message,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, // allows clickable hyperlinks
                replyMarkup: replyMarkup
            );

            Console.WriteLine("SENT --- " + message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending Telegram message: {ex.Message}");
        }
    }

    public async Task InitializeLastUpdateId()
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"{_telegramApiUrl}/getUpdates");
            var updates = JsonSerializer.Deserialize<UpdateResponse>(response);

            if (updates?.result != null && updates.result.Length > 0)
            {
                _lastUpdateId = updates.result[^1].update_id; // Set to the last update ID
                Console.WriteLine($"Initialized _lastUpdateId: {_lastUpdateId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing last update ID: {ex.Message}");
        }
    }

    public async Task<string?> ListenForCommands()
    {
        var message = "";
        try
        {
            var response =
                await _httpClient.GetStringAsync($"{_telegramApiUrl}/getUpdates?offset={_lastUpdateId + 1}");
            var updates = JsonSerializer.Deserialize<UpdateResponse>(response);

            if (updates?.result != null && updates.result.Length > 0)
            {
                var lastUpdate = updates.result[^1];

                if (lastUpdate.update_id == _lastUpdateId)
                    return "";

                if (lastUpdate.update_id > _lastUpdateId)
                {
                    _lastUpdateId = lastUpdate.update_id;

                    message = lastUpdate.message.text.Trim().ToLower();

                    return message;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching updates: {ex.Message}");
        }

        return "";
    }
}