using SteamKit2;
using System.Text.Json.Serialization;

namespace ASF_Bot.Data.Responses;
public sealed record GetBotResponse : AbstractResponse<Dictionary<string, GetBotResponse.ResultData>>
{
    public sealed record ResultData
    {
        public string? BotName { get; set; }

        public CardsFarmerData? CardsFarmer { get; set; }

        [JsonPropertyName("s_SteamID")]
        public string? SteamId { get; set; }

        public bool HasMobileAuthenticator { get; set; }
        public bool IsConnectedAndLoggedOn { get; set; }

        public bool IsPlayingPossible { get; set; }

        public bool KeepRunning { get; set; }
        public string? Nickname { get; set; }
        public EAccountFlags AccountFlags { get; set; }
    }

    public sealed record CardsFarmerData
    {
        public List<GameData>? CurrentGamesFarming { get; set; }
        public List<GameData>? GamesToFarm { get; set; }
        public string? TimeRemaining { get; set; }
        public bool Paused { get; set; }
    }

    public sealed record GameData
    {
        [JsonPropertyName("AppID")]
        public uint AppId { get; set; }
        public string? GameName { get; set; }
        public byte CardsRemaining { get; set; }
        public double HoursPlayed { get; set; }
    }
}
