namespace ASF_Bot.Infrastructure.Configs;

/// <summary>
/// 系统选项
/// </summary>
public sealed record SystemConfig
{
    public bool Debug { get; set; } = BuildInfo.IsDebug;

    public bool Statistic { get; set; } = true;

    /// <summary>
    /// 更新ASF状态间隔, 单位: 秒, 设为 0 禁用
    /// </summary>
    public int UpdateStatusInterval { get; set; } = 10;
    public int UpdateTitleInterval { get; set; } = 10;
}
