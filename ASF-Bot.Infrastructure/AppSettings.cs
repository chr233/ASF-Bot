using ASF_Bot.Infrastructure.Configs;

namespace ASF_Bot.Infrastructure;

/// <summary>
/// 机器人配置
/// </summary>
public sealed record AppSettings
{
    /// <inheritdoc cref="SystemConfig"/>
    public SystemConfig System { get; set; } = new();

    /// <inheritdoc cref="DatabaseConfig"/>
    public DatabaseConfig Database { get; set; } = new();

    /// <inheritdoc cref="TelegramConfig"/>
    public TelegramConfig Telegram { get; set; } = new();

    /// <inheritdoc cref="IpcConfig"/>
    public List<IpcConfig> AsfIpcs { get; set; } = [];
}
