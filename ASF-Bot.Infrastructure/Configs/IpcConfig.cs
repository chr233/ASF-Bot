namespace ASF_Bot.Infrastructure.Configs;

/// <summary>
/// IPC选项
/// </summary>
public sealed record IpcConfig
{
    public string? Name { get; set; }
    public string? BaseUrl { get; set; }
    public string? ApiPrefix { get; set; }
    public string? Proxy { get; set; }
    public string? IpcPassword { get; set; }
    public int Timeout { get; set; } = 60;
}
