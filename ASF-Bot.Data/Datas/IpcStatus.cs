using ASF_Bot.Data.Responses;
using System.Collections.Frozen;

namespace ASF_Bot.Data.Datas;
/// <summary>
/// 
/// </summary>
public sealed record IpcStatus
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="response"></param>
    public IpcStatus(GetBotResponse? response)
    {
        var online = 0;
        var farming = 0;
        var offline = 0;
        var disable = 0;

        Dictionary<string, BotSummary> botSummarys = [];

        if (response?.Result != null)
        {
            foreach (var (botName, data) in response.Result)
            {
                var bs = new BotSummary(data);

                if (bs.IsOnline)
                {
                    online++;
                }
                else
                {
                    offline++;
                }

                if (bs.IsFarming)
                {
                    farming++;
                }

                if (!bs.IsEnable)
                {
                    disable++;
                }
            }
        }

        OnlineBots = online;
        FarmingBots = farming;
        OfflineBots = offline;
        DisableBots = disable;
        IsConnected = response?.Result != null;
        BotsSummary = botSummarys.ToFrozenDictionary();
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsConnected { get; init; }
    /// <summary>
    /// 
    /// </summary>
    public int OnlineBots { get; init; }
    /// <summary>
    /// 
    /// </summary>
    public int FarmingBots { get; init; }
    /// <summary>
    /// 
    /// </summary>
    public int OfflineBots { get; init; }
    /// <summary>
    /// 
    /// </summary>
    public int DisableBots { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public FrozenDictionary<string, BotSummary> BotsSummary { get; init; }

    /// <inheritdoc/>
    public override string? ToString()
    {
        if (!IsConnected)
        {
            return "连接失败 / 未连接";
        }
        else
        {
            return $"挂卡 {FarmingBots} 在线 {OnlineBots}  离线 {OfflineBots}";
        }
    }
}
