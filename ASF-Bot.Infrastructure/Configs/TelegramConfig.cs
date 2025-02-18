namespace ASF_Bot.Infrastructure.Configs;

/// <summary>
/// 机器人选项
/// </summary>
public sealed record TelegramConfig
{
    public string? BotToken { get; init; }
    public string? Proxy { get; init; }
    public string? BaseUrl { get; init; }
    public bool DropPendingUpdates { get; init; }
    public List<long>? AdminUsers { get; init; }
}
