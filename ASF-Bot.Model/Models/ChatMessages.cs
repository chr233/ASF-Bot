using ASF_Bot.Model.Base;
using ASF_Bot.Model.Columns;
using SqlSugar;

namespace ASF_Bot.Model.Models;

/// <summary>
/// 用户信息表
/// </summary>
[SugarTable("chat_message", TableDescription = "消息列表")]
public sealed record ChatMessages : BaseModel, IExpiredAt
{
    public long ChatId { get; set; }

    public int MessageId { get; set; }

    /// <inheritdoc />
    public DateTime ExpiredAt { get; set; }
}
