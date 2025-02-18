using ASF_Bot.Data.Responses;
using System.Collections.Frozen;

namespace ASF_Bot.Data.Datas;
public sealed record IpcStatus
{
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

    public bool IsConnected { get; init; }
    public int OnlineBots { get; init; }
    public int FarmingBots { get; init; }
    public int OfflineBots { get; init; }
    public int DisableBots { get; init; }

    public FrozenDictionary<string, BotSummary> BotsSummary { get; init; }

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
