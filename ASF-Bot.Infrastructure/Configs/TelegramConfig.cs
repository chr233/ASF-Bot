namespace ASF_Bot.Infrastructure.Configs;

/// <summary>
/// 机器人选项
/// </summary>
public sealed record TelegramConfig
{
    public string? BotToken { get; set; }
    public string? Proxy { get; set; }
    public string? BaseUrl { get; set; }
    public bool DropPendingUpdates { get; set; }
    public List<string>? AdminUsers { get; set; }
}
