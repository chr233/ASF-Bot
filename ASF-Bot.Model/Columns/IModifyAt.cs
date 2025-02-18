namespace ASF_Bot.Model.Columns;

/// <summary>
/// 记录修改时间
/// </summary>
public interface IModifyAt
{
    /// <inheritdoc cref="IModifyAt"/>
    DateTime ModifyAt { get; set; }
}
