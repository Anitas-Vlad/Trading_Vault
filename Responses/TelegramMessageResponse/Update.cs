namespace TradingVault.Responses.TelegramMessageResponse;

public class Update
{
    public int update_id { get; set; }
    public Message message { get; set; } = default!; // Assuming a message is always present
}