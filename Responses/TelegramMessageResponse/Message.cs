namespace TradingVault.Responses.TelegramMessageResponse;

public class Message
{
    public int message_id { get; set; }
    public User from { get; set; } = default!;
    public Chat chat { get; set; } = default!;
    public int date { get; set; }
    public string text { get; set; } = default!;
}