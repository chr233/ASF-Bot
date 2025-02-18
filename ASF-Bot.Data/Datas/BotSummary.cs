using ASF_Bot.Data.Responses;
using SteamKit2;

namespace ASF_Bot.Data.Datas;
public sealed record BotSummary
{
    public BotSummary(GetBotResponse.ResultData result)
    {
        IsOnline = result.IsConnectedAndLoggedOn;
        IsEnable = result.KeepRunning;

        IsFarming = result.CardsFarmer?.Paused == false
            && result.CardsFarmer?.TimeRemaining != "00:00:00"
            && result.CardsFarmer?.CurrentGamesFarming?.Count > 0;

        IsLocked = result.AccountFlags.HasFlag(EAccountFlags.LimitedUser);

        Nickname = result.Nickname;

        if (ulong.TryParse(result.SteamId, out var steamId))
        {
            SteamId = steamId;
        }
    }

    public bool IsFarming { get; set; }
    public bool IsOnline { get; set; }
    public bool IsEnable { get; set; }
    public bool IsLocked { get; set; }
    public string? Nickname { get; set; }
    public ulong SteamId { get; init; }
}
