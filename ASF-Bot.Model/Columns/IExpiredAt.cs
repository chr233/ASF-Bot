namespace ASF_Bot.Model.Columns;

/// <summary>
/// 记录过期时间
/// </summary>
public interface IExpiredAt
{
    /// <inheritdoc cref="IExpiredAt"/>
    DateTime ExpiredAt { get; set; }
}
