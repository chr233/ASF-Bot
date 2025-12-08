using ASF_Bot.Data.Responses;
using SteamKit2;

namespace ASF_Bot.Data.Datas;
/// <summary>
/// 
/// </summary>
public sealed record BotSummary
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
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

    /// <summary>
    /// 
    /// </summary>
    public bool IsFarming { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public bool IsOnline { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public bool IsEnable { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public bool IsLocked { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? Nickname { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public ulong SteamId { get; init; }
}
