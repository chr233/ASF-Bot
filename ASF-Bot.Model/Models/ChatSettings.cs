using ASF_Bot.Model.Base;
using ASF_Bot.Model.Columns;
using SqlSugar;

namespace ASF_Bot.Model.Models;

/// <summary>
/// 用户信息表
/// </summary>
[SugarTable("chat_setting", TableDescription = "对话列表")]
[SugarIndex("i_tg_chats", nameof(ChatId), OrderByType.Asc, nameof(ThreadId), OrderByType.Asc, true)]
public sealed record ChatSettings : BaseModel, IModifyAt, ICreateAt
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(IsNullable = true)]
    public long? ChatId { get; set; }

    [SugarColumn(IsNullable = true)]
    public int? ThreadId { get; set; }

    [SugarColumn(IsNullable = true, Length = 100)]
    public int? IpcId { get; set; }

    public bool IsUpdateTitle { get; set; } = false;

    public bool IsTopidEnable { get; set; } = false;

    [SugarColumn(IsNullable = true, Length = 200)]
    public string? Title { get; set; }

    /// <inheritdoc />
    public DateTime CreateAt { get; set; }
    /// <inheritdoc />
    public DateTime ModifyAt { get; set; }

}
