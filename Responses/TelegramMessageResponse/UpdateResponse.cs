namespace TradingVault.Responses.TelegramMessageResponse;

public class UpdateResponse
{
    public bool ok { get; set; } // To capture the "ok" status
    public Update[]? result { get; set; }
}