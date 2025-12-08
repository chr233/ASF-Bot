using SteamKit2;
using System.Text.Json.Serialization;

namespace ASF_Bot.Data.Responses;
/// <summary>
/// 
/// </summary>
public sealed record GetBotResponse : AbstractResponse<Dictionary<string, GetBotResponse.ResultData>>
{
    /// <summary>
    /// 
    /// </summary>
    public sealed record ResultData
    {
        /// <summary>
        /// 
        /// </summary>
        public string? BotName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public CardsFarmerData? CardsFarmer { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("s_SteamID")]
        public string? SteamId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool HasMobileAuthenticator { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsConnectedAndLoggedOn { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsPlayingPossible { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool KeepRunning { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? Nickname { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public EAccountFlags AccountFlags { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed record CardsFarmerData
    {
        /// <summary>
        /// 
        /// </summary>
        public List<GameData>? CurrentGamesFarming { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<GameData>? GamesToFarm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? TimeRemaining { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Paused { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed record GameData
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("AppID")]
        public uint AppId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? GameName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public byte CardsRemaining { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double HoursPlayed { get; set; }
    }
}
