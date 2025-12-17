using ASF_Bot.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ASF_Bot.Service.Telegram.Service;

public sealed class ValidUserService
{
    private readonly ILogger<UpdateService> _logger;
    private readonly HashSet<long> AdminUsers = [];

    public ValidUserService(
        ILogger<UpdateService> logger,
        IOptions<AppSettings> _options)
    {
        _logger = logger;

        if (_options.Value.Telegram?.AdminUsers != null)
        {
            foreach (var userId in _options.Value.Telegram.AdminUsers)
            {
                if (long.TryParse(userId, out var id))
                {
                    AdminUsers.Add(id);
                }
                else
                {
                    _logger.LogWarning("管理员用户ID无效: {userId}", userId);
                }
            }
        }

        if (AdminUsers.Count == 0)
        {
            _logger.LogWarning("未配置任何管理员, 将无法使用任何命令, 请修改 Telegram.AdminUsers 项");
        }
    }

    public bool ValidUser(Update update)
    {
        var msgUser = update.Type switch {
            UpdateType.ChannelPost => update.ChannelPost!.From,
            UpdateType.EditedChannelPost => update.EditedChannelPost!.From,
            UpdateType.Message => update.Message!.From,
            UpdateType.EditedMessage => update.EditedMessage!.From,
            UpdateType.CallbackQuery => update.CallbackQuery!.From,
            UpdateType.InlineQuery => update.InlineQuery!.From,
            UpdateType.ChosenInlineResult => update.ChosenInlineResult!.From,
            _ => null
        };

        if (msgUser == null)
        {
            return false;
        }

        return AdminUsers.Contains(msgUser.Id);
    }

    public bool ValidUser(Message message)
    {
        if (message.From == null)
        {
            return false;
        }

        return AdminUsers.Contains(message.From.Id);
    }

    public bool ValidUser(User? user)
    {
        if (user == null)
        {
            return false;
        }

        return AdminUsers.Contains(user.Id);
    }
}
