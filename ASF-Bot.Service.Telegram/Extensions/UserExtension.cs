using Telegram.Bot.Types;

namespace ASF_Bot.Service.Telegram.Extensions;

/// <summary>
/// User扩展
/// </summary>
public static class UserExtension
{
    /// <summary>
    /// 获取用户全名
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string FullName(this User user)
    {
        return string.IsNullOrEmpty(user.LastName) ? user.FirstName : $"{user.FirstName} {user.LastName}";
    }

    /// <summary>
    /// 获取用户ID
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string UserID(this User user)
    {
        return string.IsNullOrEmpty(user.Username) ? $"#{user.Id}" : $"@{user.Username} #{user.Id}";
    }

    /// <summary>
    /// 获取用户摘要信息
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string UserProfile(this User user)
    {
        return $"{user.FullName()} {user.UserID()}";
    }

    /// <summary>
    /// 打印用户信息
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string UserToString(this User user)
    {
        return $"{user.FullName()}({user.UserID()})";
    }

    /// <summary>
    /// Html格式的用户链接
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string HtmlUserLink(this User user)
    {
        var nick = user.FullName();

        return string.IsNullOrEmpty(user.Username)
            ? $"<a href=\"tg://user?id={user.UserID}\">{nick}</a>"
            : $"<a href=\"https://t.me/{user.Username}\">{nick}</a>";
    }
}
