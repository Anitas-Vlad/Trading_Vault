namespace TradingVault.Responses.TelegramMessageResponse;

public class User
{
    public long id { get; set; }
    public bool is_bot { get; set; }
    public string first_name { get; set; } = default!;
    public string language_code { get; set; } = default!;
}